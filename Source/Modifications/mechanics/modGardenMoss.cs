using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using ExtendedVariants.Entities.Legacy;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modGardenMoss : IGameModification
    {

        private static Type ExtendedVariantTriggerType;

        private static Hook HookEnter;
        private static Hook HookLeave;
        private delegate void orig_OnEnter(ExtendedVariantTrigger self, Player player);
        private delegate void orig_OnLeave(ExtendedVariantTrigger self, Player player);

        private static FieldInfo VariantChangeField;
        
        public override void Load()
        {
            On.Celeste.Decal.Render += OnDecalRender;

            ExtendedVariantTriggerType = typeof(ExtendedVariantTrigger);
            VariantChangeField = ExtendedVariantTriggerType.GetField("variantChange", BindingFlags.Instance | BindingFlags.NonPublic);

            HookEnter = new Hook(ExtendedVariantTriggerType.GetMethod("OnEnter", BindingFlags.Public | BindingFlags.Instance),
                typeof(modGardenMoss).GetMethod(nameof(modOnEnter), BindingFlags.Static | BindingFlags.NonPublic));
            HookLeave = new Hook(ExtendedVariantTriggerType.GetMethod("OnLeave", BindingFlags.Public | BindingFlags.Instance),
                typeof(modGardenMoss).GetMethod(nameof(modOnLeave), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            On.Celeste.Decal.Render -= OnDecalRender;

            HookEnter?.Dispose();
            HookLeave?.Dispose();
            HookEnter = null;
            HookLeave = null;
        }

        private static void modOnEnter(orig_OnEnter orig, ExtendedVariantTrigger self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || !isInGarden() || isActive(self))
            {
                orig(self, player);
            }
        }

        private static void modOnLeave(orig_OnLeave orig, ExtendedVariantTrigger self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || !isInGarden() || isActive(self))
            {
                orig(self, player);
            }
        }

        private static bool isActive(ExtendedVariantTrigger self)
        {
            ExtendedVariantsModule.Variant variant = (ExtendedVariantsModule.Variant)VariantChangeField.GetValue(self);

            return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_BOUNCE_MOSS) && variant == ExtendedVariantsModule.Variant.BounceEverywhere ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_SPEED_MOSS) && (variant == ExtendedVariantsModule.Variant.HyperdashSpeed || variant == ExtendedVariantsModule.Variant.WallBouncingSpeed);
        }

        private static bool isInGarden()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/4-Expert/DanTKO") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("f02_dantko");
        }

        private static void OnDecalRender(On.Celeste.Decal.orig_Render orig, Decal self)
        {
            if(!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }

            if ((self.Name.Contains("moss_Ablue") || self.Name.Contains("moss_Bblue") || self.Name.Contains("moss_Cblue")) &&
                !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_BOUNCE_MOSS) ||
                (self.Name.Contains("moss_Ared") || self.Name.Contains("moss_Bred") || self.Name.Contains("moss_Cred")) &&
                !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_SPEED_MOSS))
            {
                Color oldColor = self.Color;
                self.Color = Color.Black;
                orig(self);
                self.Color = oldColor;
                return;
            }
            else orig(self);

        }
    }
}
