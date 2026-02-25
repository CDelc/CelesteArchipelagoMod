using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modPufferFish : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Puffer.Update += modPuffer_Update;
        }

        public override void Unload()
        {
            On.Celeste.Puffer.Update -= modPuffer_Update;
        }

        private static void modPuffer_Update(On.Celeste.Puffer.orig_Update orig, Puffer self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PUFFER_FISH))
            {
                orig(self);
            }
            else
            {
                self.Collidable = false;
                self.goneTimer = 2.5f;
                self.state = Puffer.States.Gone;
            }
        }
    }
}
