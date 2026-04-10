using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.EmHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modMonumentSwitch : IGameModification
    {

        private static Type MonumentFlipSwitchType;
        private static Type MonumentSwitchBlockType;

        private static FieldInfo ActivatorField;
        private static FieldInfo BlockActivatorField;
        private static FieldInfo SpriteField;

        private static MethodInfo ShiftSizeMethod;
        private static MethodInfo UpdateVisualStateMethod;

        private static Hook UpdateHook;
        private delegate void orig_Update(MonumentFlipSwitch self);

        private static Hook BlockUpdateHook;
        private delegate void orig_UpdateBlock(MonumentSwitchBlock self);

        private static Color BlueColor = Calc.HexToColor("82d9ff");
        private static Color GreenColor = Calc.HexToColor("8aff82");
        private static Color PurpleColor = Calc.HexToColor("c382ff");
        private static Color PurpleColorTwo = Calc.HexToColor("9582ff");
        private static Color RedColor = Calc.HexToColor("ff8282");
        private static Color PinkColor = Calc.HexToColor("ff82de");

        public override void Load()
        {
            MonumentFlipSwitchType = typeof(MonumentFlipSwitch);
            MonumentSwitchBlockType = typeof(MonumentSwitchBlock);
            BlockActivatorField = MonumentSwitchBlockType.GetField("activator", BindingFlags.NonPublic | BindingFlags.Instance);
            ActivatorField = MonumentFlipSwitchType.GetField("activator", BindingFlags.NonPublic | BindingFlags.Instance);
            SpriteField = MonumentFlipSwitchType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);

            ShiftSizeMethod = MonumentSwitchBlockType.GetMethod("ShiftSize", BindingFlags.Instance | BindingFlags.NonPublic);
            UpdateVisualStateMethod = MonumentSwitchBlockType.GetMethod("UpdateVisualState", BindingFlags.Instance | BindingFlags.NonPublic);

            UpdateHook = new Hook(MonumentFlipSwitchType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modMonumentSwitch).GetMethod(nameof(modUpdate), BindingFlags.NonPublic | BindingFlags.Static));
            BlockUpdateHook = new Hook(MonumentSwitchBlockType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modMonumentSwitch).GetMethod(nameof(modBlockUpdate), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            UpdateHook?.Dispose();
            UpdateHook = null;
            BlockUpdateHook?.Dispose();
            BlockUpdateHook = null;
        }

        private static void modUpdate(orig_Update orig, MonumentFlipSwitch self)
        {
            if(CelesteArchipelagoModule.shouldModMechanics && !isActive(self))
            {
                self.Collidable = false;
                Sprite sprite = (Sprite)SpriteField.GetValue(self);
                sprite.Color.A = 30;
                sprite.Color.R = 50;
                sprite.Color.G = 0;
                sprite.Color.B = 0;
            }
            orig(self);
        }

        private static void modBlockUpdate(orig_UpdateBlock orig, MonumentSwitchBlock self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !isActive(self))
            {
                bool orig_Activated = ((MonumentActivator)BlockActivatorField.GetValue(self)).Activated;
                ((MonumentActivator)BlockActivatorField.GetValue(self)).Activated = false;
                orig(self);
                ((MonumentActivator)BlockActivatorField.GetValue(self)).Activated = orig_Activated;
            }
            else orig(self);
        }

        private static bool isActive(MonumentFlipSwitch self)
        {
            Color color = ((MonumentActivator)ActivatorField.GetValue(self)).Index;
            return color.Equals(BlueColor) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_FLIP_SWITCH) ||
                color.Equals(GreenColor) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_FLIP_SWITCH) ||
                (color.Equals(PurpleColor) || color.Equals(PurpleColorTwo)) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_FLIP_SWITCH) ||
                color.Equals(RedColor) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_FLIP_SWITCH) ||
                color.Equals(PinkColor) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_FLIP_SWITCH);
        }

        private static bool isActive(MonumentSwitchBlock self)
        {
            Color color = ((MonumentActivator)BlockActivatorField.GetValue(self)).Index;
            return (color.Equals(PurpleColor) || color.Equals(PurpleColorTwo)) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_SWITCH_BLOCK) ||
                color.Equals(RedColor) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_SWITCH_BLOCK) ||
                color.Equals(PinkColor) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_SWITCH_BLOCK);
        }
    }
}
