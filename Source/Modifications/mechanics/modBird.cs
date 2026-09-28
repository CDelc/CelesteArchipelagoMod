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
            On.Celeste.FlingBirdIntro.OnPlayer += modFlingBirdIntro_OnPlayer;
            On.Celeste.FlingBirdIntro.Update += modFlingBirdIntro_Update;
        }

        public override void Unload()
        {
            On.Celeste.FlingBird.OnPlayer -= modFlingBird_OnPlayer;
            On.Celeste.FlingBird.Render -= modFlingBird_Render;
            On.Celeste.FlingBirdIntro.OnPlayer -= modFlingBirdIntro_OnPlayer;
            On.Celeste.FlingBirdIntro.Update -= modFlingBirdIntro_Update;
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

        private void modFlingBirdIntro_OnPlayer(On.Celeste.FlingBirdIntro.orig_OnPlayer orig, FlingBirdIntro self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.FLYING_BIRD))
            {
                orig(self, player);
            }
        }

        private static void modFlingBirdIntro_Update(On.Celeste.FlingBirdIntro.orig_Update orig, FlingBirdIntro self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.FLYING_BIRD))
            {
                self.Sprite.Color.R = (byte)(0.7f * 255.0f);
                self.Sprite.Color.G = (byte)(0.0f * 255.0f);
                self.Sprite.Color.B = (byte)(0.0f * 255.0f);
                self.Sprite.Color.A = (byte)(0.2f * 255.0f);
            }
            else
            {
                self.Sprite.Color.R = (byte)255;
                self.Sprite.Color.G = (byte)255;
                self.Sprite.Color.B = (byte)255;
                self.Sprite.Color.A = (byte)255;
            }
        }
    }
}
