using Celeste.Mod.BounceHelper;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.DJMapHelper.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.StrawberryJam2021.Entities;
using FrostHelper;
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
    internal class modDashCrystal : IGameModification
    {

        private static Type DashCrystalType;
        private static Type JumpRefillType;
        private static Type DreamRefillType;
        private static Type ExpiringDashRefillType;
        private static Type ResettingRefillType;
        private static Type PlusOneRefillType;
        private static Type BounceRefillType;
        private static Type CustomizableRefillType;

        private static FieldInfo ExtraJumpsField;
        private static FieldInfo numDashesField;
        private static FieldInfo ExtraJumpFieldBool;

        private static FieldInfo SpriteField;
        private static FieldInfo OutlineField;
        private static FieldInfo RecoverStaminaField;
        private static Hook PlusOneRefillHook;

        private static Hook BounceRefillHook;
        private static FieldInfo TwoDashesBounceField;
        private static FieldInfo OutlineBounceField;
        private static FieldInfo SpriteBounceField;

        private delegate void orig_RenderPlusOne(PlusOneRefill self);
        private delegate void orig_RenderBounceRefill(BounceRefill self);

        public override void Load()
        {
            DashCrystalType = typeof(Refill);
            JumpRefillType = CelesteArchipelagoModule.FindType("ExtendedVariants.Entities.ForMappers.JumpRefill");
            DreamRefillType = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.DashStates.DreamTunnelRefill");
            ExpiringDashRefillType = typeof(ExpiringDashRefill);
            ResettingRefillType = typeof(ResettingRefill);
            PlusOneRefillType = typeof(PlusOneRefill);
            BounceRefillType = typeof(BounceRefill);
            CustomizableRefillType = typeof(CustomizableRefill);

            numDashesField = ResettingRefillType.GetField("dashes", BindingFlags.NonPublic | BindingFlags.Instance);
            ExtraJumpsField = JumpRefillType.GetField("extraJumps", BindingFlags.NonPublic | BindingFlags.Instance);
            ExtraJumpFieldBool = ResettingRefillType.GetField("extraJump", BindingFlags.NonPublic | BindingFlags.Instance);

            SpriteField = PlusOneRefillType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            OutlineField = PlusOneRefillType.GetField("outline", BindingFlags.NonPublic | BindingFlags.Instance);
            RecoverStaminaField = PlusOneRefillType.GetField("recoverStamina", BindingFlags.NonPublic | BindingFlags.Instance);

            TwoDashesBounceField = BounceRefillType.GetField("twoDashes", BindingFlags.NonPublic | BindingFlags.Instance);
            OutlineBounceField = BounceRefillType.GetField("outline", BindingFlags.NonPublic | BindingFlags.Instance);
            SpriteBounceField = BounceRefillType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);

            On.Celeste.Refill.Render += modRefill_Render;
            PlusOneRefillHook = new Hook(PlusOneRefillType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance),
                typeof(modDashCrystal).GetMethod(nameof(modRenderPlusOneRefill), BindingFlags.NonPublic | BindingFlags.Static));
            BounceRefillHook = new Hook(BounceRefillType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance),
                typeof(modDashCrystal).GetMethod(nameof(modRenderBounceRefill), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            On.Celeste.Refill.Render -= modRefill_Render;
            PlusOneRefillHook?.Dispose();
            PlusOneRefillHook = null;
            BounceRefillHook?.Dispose();
            BounceRefillHook = null;
        }

        private static void modRefill_Render(On.Celeste.Refill.orig_Render orig, Refill self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            if (!isActive(self))
            {
                self.outline.Visible = true;
                self.sprite.Visible = false;
                self.Collidable = false;
                self.respawnTimer = 2.5f;
            }
            orig(self);
        }

        private static void modRenderPlusOneRefill(orig_RenderPlusOne orig, PlusOneRefill self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            if (!isActive(self))
            {
                Image outline = (Image)OutlineField.GetValue(self);
                Sprite sprite = (Sprite)SpriteField.GetValue(self);
                outline.Visible = true;
                sprite.Visible = false;
                self.Collidable = false;

                OutlineField.SetValue(self, outline);
                SpriteField.SetValue(self, sprite);
            }
            orig(self);
        }

        private static void modRenderBounceRefill(orig_RenderBounceRefill orig, BounceRefill self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            if (!isActive(self))
            {
                Image outline = (Image)OutlineBounceField.GetValue(self);
                Sprite sprite = (Sprite)SpriteBounceField.GetValue(self);
                outline.Visible = true;
                sprite.Visible = false;
                self.Collidable = false;

                OutlineBounceField.SetValue(self, outline);
                SpriteBounceField.SetValue(self, sprite);
            }
            orig(self);
        }

        private static bool isActive(Entity self)
        {
            int extraJumps = 0;
            int numDashes = 0;
            bool extraJump = false;
            bool twoDashes = false;
            bool recoverStamina = true;
            if(self.GetType() == JumpRefillType)
            {
                extraJumps = (int)ExtraJumpsField.GetValue(self);
            }
            if(self.GetType() == ResettingRefillType)
            {
                numDashes = (int)numDashesField.GetValue(self);
            }
            if (self.GetType() == ResettingRefillType)
            {
                extraJump = (bool)ExtraJumpFieldBool.GetValue(self);
            }
            if (self.GetType() == PlusOneRefillType)
            {
                recoverStamina = (bool)RecoverStaminaField.GetValue(self);
            }
            if (self.GetType().IsAssignableFrom(DashCrystalType))
            {
                twoDashes = ((Refill)self).twoDashes;
            }
            else if(self.GetType() == BounceRefillType)
            {
                twoDashes = (bool)TwoDashesBounceField.GetValue(self);
            }

            return self.GetType() == DashCrystalType && twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_CRYSTAL) ||
                self.GetType() == DashCrystalType && !twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS) ||
                self.GetType() == CustomizableRefillType && !twoDashes && !isMosaicCrystal(self) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS) ||
                self.GetType() == CustomizableRefillType && !twoDashes && isMosaicCrystal(self) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.INFINITE_DASH_CRYSTAL) ||
                self.GetType() == CustomizableRefillType && twoDashes && !isMosaicCrystal(self) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_CRYSTAL) ||
                self.GetType() == ResettingRefillType && numDashes == 2 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_CRYSTAL) ||
                self.GetType() == ResettingRefillType && numDashes == 1 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS) ||
                self.GetType() == typeof(ColorfulRefill) && numDashes == 1 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS) ||
                self.GetType() == ResettingRefillType && numDashes == 0 && extraJump && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SINGLE_JUMP_REFILL) ||
                self.GetType() == DreamRefillType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_DASH_CRYSTALS) ||
                self.GetType() == JumpRefillType && extraJumps == 1 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SINGLE_JUMP_REFILL) ||
                self.GetType() == JumpRefillType && extraJumps == 3 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TRIPLE_JUMP_REFILL) ||
                self.GetType() == ExpiringDashRefillType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS) ||
                self.GetType() == PlusOneRefillType && recoverStamina && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS) ||
                self.GetType() == PlusOneRefillType && !recoverStamina && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.NO_STAMINA_DASH_CRYSTAL) ||
                self.GetType() == BounceRefillType && twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_CRYSTAL) ||
                self.GetType() == BounceRefillType && !twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS);
        }

        private static bool isMosaicCrystal(Entity self)
        {
            return self.GetType() == CustomizableRefillType &&
                (SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/4-Expert/Scroogle") || SaveData.Instance.CurrentSession_Safe.Level.Equals("e04_scroogle")) &&
                !((CustomizableRefill)self).oneUse;
        }
    }
}
