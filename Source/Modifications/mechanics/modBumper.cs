using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.JackalHelper.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.VortexHelper.Entities;
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
    internal class modBumper : IGameModification
    {
        BindingFlags privateLookup = BindingFlags.Instance | BindingFlags.NonPublic;
        BindingFlags staticLookup = BindingFlags.Static | BindingFlags.NonPublic;

        private static Type CardinalBumperType;
        private static Hook CardinalUpdateHook;
        private static FieldInfo CardinalSpriteField;
        private static FieldInfo CardinalRespawnTimerField;
        private delegate void orig_CardinalUpdate(CardinalBumper self);

        private static Type StaticBumperType;
        private static Hook StaticBumperHook;
        private static FieldInfo StaticSpriteField;
        private static FieldInfo StaticRespawnField;
        private delegate void orig_StaticUpdate(StaticBumper self);

        private static Type VortexBumperType;
        private static Hook VortexBumperHook;
        private static FieldInfo VortexSpriteField;
        private static FieldInfo VortexRespawnField;
        private static FieldInfo VortexOneUseField;
        private static FieldInfo VortexTwoDashField;
        private delegate void orig_VortexUpdate(VortexBumper self);

        public override void Load()
        {
            CardinalBumperType = typeof(CardinalBumper);
            CardinalSpriteField = CardinalBumperType.GetField("sprite", privateLookup);
            CardinalRespawnTimerField = CardinalBumperType.GetField("respawnTimer", privateLookup);

            StaticBumperType = typeof(StaticBumper);
            StaticSpriteField = StaticBumperType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            StaticRespawnField = StaticBumperType.GetField("respawnTimer", BindingFlags.NonPublic | BindingFlags.Instance);

            VortexBumperType = typeof(VortexBumper);
            VortexSpriteField = VortexBumperType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            VortexRespawnField = VortexBumperType.GetField("respawnTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            VortexOneUseField = VortexBumperType.GetField("oneUse", BindingFlags.NonPublic | BindingFlags.Instance);
            VortexTwoDashField = VortexBumperType.GetField("twoDashes", BindingFlags.NonPublic | BindingFlags.Instance);

            StaticBumperHook = new Hook(StaticBumperType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modBumper).GetMethod(nameof(modStaticBumper_Update), staticLookup));
            VortexBumperHook = new Hook(VortexBumperType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modBumper).GetMethod(nameof(modVortexBumper_Update), staticLookup));
            CardinalUpdateHook = new Hook(CardinalBumperType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modBumper).GetMethod(nameof(modCardinalBumper_Update), staticLookup));
            
            On.Celeste.Bumper.Update += modBumper_Update;
        }

        public override void Unload()
        {
            On.Celeste.Bumper.Update -= modBumper_Update;
            
            CardinalUpdateHook?.Dispose();
            CardinalUpdateHook = null;
            StaticBumperHook?.Dispose();
            StaticBumperHook = null;
            VortexBumperHook?.Dispose();
            VortexBumperHook = null;
        }

        private static void modBumper_Update(On.Celeste.Bumper.orig_Update orig, Bumper self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !BumperIsActive(self))
            {
                self.respawnTimer = 0.6f;
                self.sprite.Play("hit", false, false);
            }

            orig(self);
        }

        private static bool BumperIsActive(Bumper self)
        {
            return self.GetType() != typeof(MultiNodeBumper) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BUMPER) ||
                self.GetType() == typeof(MultiNodeBumper) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_BUMPER);
        }

        private static void modStaticBumper_Update(orig_StaticUpdate orig, StaticBumper self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BUMPER))
            {
                StaticRespawnField.SetValue(self, 0.6f);
                ((Sprite)StaticSpriteField.GetValue(self)).Play("hit", false, false);
            }

            orig(self);
        }

        private static void modVortexBumper_Update(orig_VortexUpdate orig, VortexBumper self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !VortexIsActive(self))
            {
                VortexRespawnField.SetValue(self, 0.6f);
                ((Sprite)VortexSpriteField.GetValue(self)).Play("hit", false, false);
            }

            orig(self);
        }

        private static bool VortexIsActive(VortexBumper self)
        {
            bool oneUse = (bool)VortexOneUseField.GetValue(self);
            bool twoDashes = (bool)VortexTwoDashField.GetValue(self);
            return oneUse && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.ONE_USE_BUMPER) ||
                twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_BUMPER);
        }

        private static void modCardinalBumper_Update(orig_CardinalUpdate orig, CardinalBumper self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SQUARE_BUMPER))
            {
                CardinalRespawnTimerField.SetValue(self, 0.6f);
                ((Sprite)CardinalSpriteField.GetValue(self)).Play("hit", false, false);
            }

            orig(self);
        }
    }
}
