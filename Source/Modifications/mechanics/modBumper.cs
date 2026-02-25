using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modBumper : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Bumper.Update += modBumper_Update;
        }

        public override void Unload()
        {
            On.Celeste.Bumper.Update -= modBumper_Update;
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
    }
}
