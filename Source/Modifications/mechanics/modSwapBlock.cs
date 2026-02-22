using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modSwapBlock : IGameModification
    {
        public override void Load()
        {
            On.Celeste.SwapBlock.Update += modSwapBlock_Update;
            On.Celeste.SwapBlock.OnDash += modSwapBlock_OnDash;
        }

        public override void Unload()
        {
            On.Celeste.SwapBlock.Update -= modSwapBlock_Update;
            On.Celeste.SwapBlock.OnDash -= modSwapBlock_OnDash;
        }

        private static void modSwapBlock_Update(On.Celeste.SwapBlock.orig_Update orig, SwapBlock self)
        {
            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SWAP_BLOCK) && CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                self.redAlpha = 0.0f;
            }

            orig(self);
        }

        private static void modSwapBlock_OnDash(On.Celeste.SwapBlock.orig_OnDash orig, SwapBlock self, Vector2 direction)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SWAP_BLOCK))
            {
                orig(self, direction);
            }
        }
    }
}
