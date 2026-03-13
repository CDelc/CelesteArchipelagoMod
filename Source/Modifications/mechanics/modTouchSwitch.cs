using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.OutbackHelper;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using vitmod;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modTouchSwitch : IGameModification
    {

        private static Type FlagTouchSwitchType;
        private static Type TimedTouchSwitchType;
        private static Type MovingTouchSwitchType;
        private static Type VanillaTouchSwitchType;
        private static Type CustomMovingTouchSwitchType;

        private static HashSet<Type> mappedTypes;

        private static FieldInfo InactiveColorVanilla;
        private static FieldInfo InactiveColorFlag;
        private static FieldInfo InactiveColorCustomMoving;

        private static Hook HookFlagTouchSwitchRender;
        private static Hook HookFlagTouchSwitchTurnOn;

        private static Hook HookVanillaTouchSwitchRender;
        private static Hook HookVanillaTouchSwitchTurnOn;

        private static Hook HookMovingTouchSwitchNextState;

        private static Hook HookCustomMovingTouchSwitchRender;
        private static Hook HookCustomMovingTouchSwitchTurnOn;


        private delegate void orig_Render(Entity self);
        private delegate void orig_TurnOn(Entity self);
        private delegate void orig_TurnOnBool(Entity self, bool undo);
        private delegate void orig_TriggerNextState(MovingTouchSwitch self);

        private static Color DisabledColor = Color.Red * 0.3f;

        public override void Load()
        {
            FlagTouchSwitchType = typeof(FlagTouchSwitch);
            TimedTouchSwitchType = typeof(TimedTouchSwitch);
            MovingTouchSwitchType = typeof(MovingTouchSwitch);
            VanillaTouchSwitchType = typeof(TouchSwitch);
            CustomMovingTouchSwitchType = typeof(CustomMovingTouchSwitch);

            mappedTypes = new HashSet<Type> {
                FlagTouchSwitchType,
                TimedTouchSwitchType,
                MovingTouchSwitchType,
                VanillaTouchSwitchType,
                CustomMovingTouchSwitchType
            };

            BindingFlags publicFlags = BindingFlags.Public | BindingFlags.Instance;
            BindingFlags privateFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            InactiveColorVanilla = VanillaTouchSwitchType.GetField("inactiveColor", privateFlags);
            InactiveColorFlag = FlagTouchSwitchType.GetField("inactiveColor", privateFlags); ;
            InactiveColorCustomMoving = CustomMovingTouchSwitchType.GetField("inactiveColor", privateFlags);

            MethodInfo RenderHook = typeof(modTouchSwitch).GetMethod(nameof(modRender), BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo TurnOnHook = typeof(modTouchSwitch).GetMethod(nameof(modTurnOn), BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo TurnOnHookBool = typeof(modTouchSwitch).GetMethod(nameof(modTurnOnWithBool), BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo TriggerNextStateHook = typeof(modTouchSwitch).GetMethod(nameof(modTriggerNextState), BindingFlags.NonPublic | BindingFlags.Static);

            HookFlagTouchSwitchRender = new Hook(FlagTouchSwitchType.GetMethod("Render", publicFlags), RenderHook);
            HookFlagTouchSwitchTurnOn = new Hook(FlagTouchSwitchType.GetMethod("TurnOn", publicFlags), TurnOnHook);

            HookVanillaTouchSwitchRender = new Hook(VanillaTouchSwitchType.GetMethod("Render", publicFlags), RenderHook);
            HookVanillaTouchSwitchTurnOn = new Hook(VanillaTouchSwitchType.GetMethod("TurnOn", publicFlags), TurnOnHook);

            HookCustomMovingTouchSwitchRender = new Hook(CustomMovingTouchSwitchType.GetMethod("Render", publicFlags), RenderHook);
            HookCustomMovingTouchSwitchTurnOn = new Hook(CustomMovingTouchSwitchType.GetMethod("TurnOn", publicFlags), TurnOnHookBool);

            HookMovingTouchSwitchNextState = new Hook(MovingTouchSwitchType.GetMethod("TriggerNextState", publicFlags), modTriggerNextState);
        }

        public override void Unload()
        {
            HookFlagTouchSwitchRender?.Dispose();
            HookFlagTouchSwitchTurnOn?.Dispose();
            HookVanillaTouchSwitchRender?.Dispose();
            HookVanillaTouchSwitchTurnOn?.Dispose();
            HookMovingTouchSwitchNextState?.Dispose();
            HookCustomMovingTouchSwitchRender?.Dispose();
            HookCustomMovingTouchSwitchTurnOn?.Dispose();

            HookFlagTouchSwitchRender = null;
            HookFlagTouchSwitchTurnOn = null;
            HookVanillaTouchSwitchRender = null;
            HookVanillaTouchSwitchTurnOn = null;
            HookMovingTouchSwitchNextState = null;
            HookCustomMovingTouchSwitchRender = null;
            HookCustomMovingTouchSwitchTurnOn = null;
        }

        private static void modRender(orig_Render orig, Entity self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return;
            }

            FieldInfo inactiveColorField = getInactiveColorField(self);
            if (inactiveColorField == null) return;

            if (!isActive(self))
            {
                inactiveColorField.SetValue(self, DisabledColor);
            }
        }

        private static void modTurnOn(orig_TurnOn orig, Entity self)
        {
            CelesteArchipelagoModule.Log($"{self.GetType().FullName}");
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                orig(self);
            }
        }

        private static void modTriggerNextState(orig_TriggerNextState orig, MovingTouchSwitch self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                orig(self);
            }
        }

        private static void modTurnOnWithBool(orig_TurnOnBool orig, Entity self, bool undo)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                orig(self, undo);
            }
        }

        private static bool isActive(Entity self)
        {
            return self.GetType() == VanillaTouchSwitchType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TOUCH_SWITCH) ||
                self.GetType() == FlagTouchSwitchType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TOUCH_SWITCH) ||
                self.GetType() == TimedTouchSwitchType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TIMED_TOUCH_SWITCH) ||
                self.GetType() == MovingTouchSwitchType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_TOUCH_SWITCH) ||
                self.GetType() == CustomMovingTouchSwitchType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_TOUCH_SWITCH);
        }

        private static FieldInfo getInactiveColorField(Entity self)
        {
            if (self.GetType() == VanillaTouchSwitchType)
            {
                return InactiveColorVanilla;
            }
            else if (self.GetType() == FlagTouchSwitchType)
            {
                return InactiveColorFlag;
            }
            else if (self.GetType() == TimedTouchSwitchType)
            {
                return InactiveColorVanilla;
            }
            else if (self.GetType() == MovingTouchSwitchType)
            {
                return InactiveColorVanilla;
            }
            else if (self.GetType() == CustomMovingTouchSwitchType)
            {
                return InactiveColorCustomMoving;
            }
            else return null;
        }
    }
}
