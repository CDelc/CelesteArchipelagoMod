using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modBreakerBox : IGameModification
    {
        public override void Load()
        {
            On.Celeste.LightningBreakerBox.Dashed += modLightningBreakerBox_Dashed;
            On.Celeste.LightningBreakerBox.Update += modLightningBreakerBox_Update;
        }

        public override void Unload()
        {
            On.Celeste.LightningBreakerBox.Dashed -= modLightningBreakerBox_Dashed;
            On.Celeste.LightningBreakerBox.Update -= modLightningBreakerBox_Update;
        }

        private static DashCollisionResults modLightningBreakerBox_Dashed(On.Celeste.LightningBreakerBox.orig_Dashed orig, LightningBreakerBox self, Player player, Microsoft.Xna.Framework.Vector2 dir)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BREAKER_SWITCH))
            {
                return DashCollisionResults.Bounce;
            }
            else
            {
                return orig(self, player, dir);
            }
        }

        private static void modLightningBreakerBox_Update(On.Celeste.LightningBreakerBox.orig_Update orig, LightningBreakerBox self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BREAKER_SWITCH))
            {
                self.sprite.Color.R = (byte)(0.3f * 255.0f);
                self.sprite.Color.G = (byte)(0.3f * 255.0f);
                self.sprite.Color.B = (byte)(0.3f * 255.0f);
                self.sprite.Color.A = (byte)(0.3f * 255.0f);
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
