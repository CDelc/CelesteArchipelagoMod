using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modWhiteBlock : IGameModification
    {

        public override void Load()
        {
            On.Celeste.WhiteBlock.Update += modWhiteBlock_Update;
        }

        public override void Unload()
        {
            On.Celeste.WhiteBlock.Update -= modWhiteBlock_Update;
        }

        private static void modWhiteBlock_Update(On.Celeste.WhiteBlock.orig_Update orig, WhiteBlock self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            
            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.WHITE_BLOCK))
            {
                self.playerDuckTimer = 0.0f;
                self.sprite.Color = Microsoft.Xna.Framework.Color.DarkRed;
            }
            else
            {
                self.sprite.Color = self.enabled ? Microsoft.Xna.Framework.Color.White : Microsoft.Xna.Framework.Color.White * 0.25f;
            }

            orig(self);
        }

    }
}
