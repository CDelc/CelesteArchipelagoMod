using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modJellyFish : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Glider.Update += modGlider_Update;
        }

        public override void Unload()
        {
            On.Celeste.Glider.Update -= modGlider_Update;
        }

        private static void modGlider_Update(On.Celeste.Glider.orig_Update orig, Glider self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.JELLYFISH))
            {
                self.Collidable = false;
                self.sprite.Color.R = (byte)(0.3f * 255.0f);
                self.sprite.Color.G = (byte)(0.3f * 255.0f);
                self.sprite.Color.B = (byte)(0.3f * 255.0f);
                self.sprite.Color.A = (byte)(0.3f * 255.0f);
                self.Hold.cannotHoldTimer = 1.0f;
            }
            else if (!self.destroyed)
            {
                self.Collidable = true;
                self.sprite.Color.R = (byte)255;
                self.sprite.Color.G = (byte)255;
                self.sprite.Color.B = (byte)255;
                self.sprite.Color.A = (byte)255;
            }
        }
    }
}
