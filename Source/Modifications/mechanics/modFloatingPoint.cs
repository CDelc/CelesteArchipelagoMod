using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using ExtendedVariants.Entities.Legacy;
using ExtendedVariants.Module;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.MaxHelpingHand.Entities;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modFloatingPoint : IGameModification
    {

        private static Type ExtendedVariantTriggerType;
        private static Type KevinBarrierType;

        private static Hook HookEnter;
        private static Hook HookLeave;
        private delegate void orig_OnEnter(ExtendedVariantTrigger self, Player player);
        private delegate void orig_OnLeave(ExtendedVariantTrigger self, Player player);

        private static Hook HookRender;
        private delegate void orig_Render(KevinBarrier self);

        private static FieldInfo NewValueField;
        private static FieldInfo ColorField;

        private static Color BlueColor = Calc.HexToColor("222244");
        private static Color RedColor = Calc.HexToColor("442222");
        private static Color PurpleColor = Calc.HexToColor("442244");

        public override void Load()
        {
            ExtendedVariantTriggerType = typeof(ExtendedVariantTrigger);
            KevinBarrierType = typeof(KevinBarrier);
            NewValueField = ExtendedVariantTriggerType.GetField("newValue", BindingFlags.Instance | BindingFlags.NonPublic);
            ColorField = KevinBarrierType.GetField("Color", BindingFlags.Instance | BindingFlags.NonPublic);

            HookEnter = new Hook(ExtendedVariantTriggerType.GetMethod("OnEnter", BindingFlags.Public | BindingFlags.Instance),
                typeof(modFloatingPoint).GetMethod(nameof(modOnEnter), BindingFlags.Static | BindingFlags.NonPublic));
            HookLeave = new Hook(ExtendedVariantTriggerType.GetMethod("OnLeave", BindingFlags.Public | BindingFlags.Instance),
                typeof(modFloatingPoint).GetMethod(nameof(modOnLeave), BindingFlags.Static | BindingFlags.NonPublic));
            HookRender = new Hook(KevinBarrierType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance),
                typeof(modFloatingPoint).GetMethod(nameof(modRender), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            HookEnter?.Dispose();
            HookLeave?.Dispose();
            HookRender?.Dispose();
            HookEnter = null;
            HookLeave = null;
            HookRender = null;
        }

        private static void modOnEnter(orig_OnEnter orig, ExtendedVariantTrigger self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || !isInFloatingPoint() || isActive(self))
            {
                orig(self, player);
            }
        }

        private static void modOnLeave(orig_OnLeave orig, ExtendedVariantTrigger self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || !isInFloatingPoint() || isActive(self))
            {
                orig(self, player);
            }
        }

        private static void modRender(orig_Render orig, KevinBarrier self)
        {
            orig(self);
            if (CelesteArchipelagoModule.shouldModMechanics && !isActive(self) && isInFloatingPoint())
            {
                Constants.DrawDisabledRect(self.Collider, Color.Black * 0.7f);
            }
        }

        private static bool isActive(ExtendedVariantTrigger self)
        {
            int gvalue = (int)NewValueField.GetValue(self);

            return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_FLOATING_FIELDS) && gvalue == -5 ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_FLOATING_FIELDS) && gvalue == -10 ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_FLOATING_FIELDS) && gvalue == -15;
        }

        private static bool isActive(KevinBarrier self)
        {
            Color color = (Color)ColorField.GetValue(self);

            return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_FLOATING_FIELDS) && color.Equals(BlueColor) ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_FLOATING_FIELDS) && color.Equals(RedColor) ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_FLOATING_FIELDS) && color.Equals(PurpleColor);
        }

        private static bool isInFloatingPoint()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/4-Expert/KAERRA") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("c05_kaerra");
        }
    }
}
