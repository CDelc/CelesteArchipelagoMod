using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modMovingPlatform : IGameModification
    {

        public override void Load()
        {
            On.Celeste.MovingPlatform.Update += modMovingPlatform_Update;
        }

        public override void Unload()
        {
            On.Celeste.MovingPlatform.Update -= modMovingPlatform_Update;
        }

        private static void modMovingPlatform_Update(On.Celeste.MovingPlatform.orig_Update orig, MovingPlatform self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
            }
            else if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_PLATFORM))
            {
                self.Collidable = true;
                orig(self);
            }
            else
            {
                Vector2 offset = new Vector2(Monocle.Calc.Random.NextFloat() * 1.5f, Monocle.Calc.Random.NextFloat() * 1.5f);
                self.Position = self.start + offset;
                self.Collidable = false;
            }
        }

    }
}
