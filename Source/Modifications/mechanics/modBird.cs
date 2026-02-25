using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modBird : IGameModification
    {
        public override void Load()
        {
            On.Celeste.FlingBird.OnPlayer += modFlingBird_OnPlayer;
            On.Celeste.FlingBird.Render += modFlingBird_Render;
        }

        public override void Unload()
        {
            On.Celeste.FlingBird.OnPlayer -= modFlingBird_OnPlayer;
            On.Celeste.FlingBird.Render -= modFlingBird_Render;
        }

        private void modFlingBird_OnPlayer(On.Celeste.FlingBird.orig_OnPlayer orig, FlingBird self, Player player)
        {
            if(!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.FLYING_BIRD))
            {
                orig(self, player);
            }
        }

        private static void modFlingBird_Render(On.Celeste.FlingBird.orig_Render orig, FlingBird self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.FLYING_BIRD))
            {
                self.sprite.Color.R = (byte)(0.7f * 255.0f);
                self.sprite.Color.G = (byte)(0.0f * 255.0f);
                self.sprite.Color.B = (byte)(0.0f * 255.0f);
                self.sprite.Color.A = (byte)(0.2f * 255.0f);
            }
            else
            {
                self.sprite.Color.R = (byte)255;
                self.sprite.Color.G = (byte)255;
                self.sprite.Color.B = (byte)255;
                self.sprite.Color.A = (byte)255;
            }
        }
    }
}
