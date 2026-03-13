using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.JackalHelper.Entities;
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

        public override void Load()
        {
            CardinalBumperType = typeof(CardinalBumper);
            CardinalSpriteField = CardinalBumperType.GetField("sprite", privateLookup);
            CardinalRespawnTimerField = CardinalBumperType.GetField("respawnTimer", privateLookup);

            CardinalUpdateHook = new Hook(CardinalBumperType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modBumper).GetMethod(nameof(modCardinalBumper_Update), staticLookup));
            
            On.Celeste.Bumper.Update += modBumper_Update;
        }

        public override void Unload()
        {
            On.Celeste.Bumper.Update -= modBumper_Update;
            
            CardinalUpdateHook?.Dispose();
            CardinalUpdateHook = null;
        }

        private static void modBumper_Update(On.Celeste.Bumper.orig_Update orig, Bumper self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BUMPER))
            {
                self.respawnTimer = 0.6f;
                self.sprite.Play("hit", false, false);
            }

            orig(self);
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
