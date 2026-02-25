using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modCoreBalls : IGameModification
    {
        public override void Load()
        {
            On.Celeste.FireBall.Update += modFireBall_Update;
        }

        public override void Unload()
        {
            On.Celeste.FireBall.Update -= modFireBall_Update;
        }

        private static void modFireBall_Update(On.Celeste.FireBall.orig_Update orig, FireBall self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.LAVA_ICE_BALLS))
            {
                self.Collidable = false;
                self.Visible = false;
            }
            else
            {
                if (!self.broken)
                {
                    self.Collidable = true;
                    self.Visible = true;
                }
            }
        }
    }
}
