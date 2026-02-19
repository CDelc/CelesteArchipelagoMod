using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modDashCrystal : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Refill.Render += modRefill_Render;
            On.Celeste.Refill.OnPlayer += modRefill_OnPlayer;
        }

        public override void Unload()
        {
            On.Celeste.Refill.Render -= modRefill_Render;
            On.Celeste.Refill.OnPlayer -= modRefill_OnPlayer;
        }

        private void modRefill_Render(On.Celeste.Refill.orig_Render orig, Refill self)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self);
                return;
            }
            else if (self.twoDashes)
            {
                //TODO: Add double dash crystal function
            }
            else if (!self.twoDashes && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS))
            {
                self.outline.Visible = true;
                self.sprite.Visible = false;
                self.Collidable = false;
                self.respawnTimer = 2.5f;
            }
            orig(self);
        }

        private void modRefill_OnPlayer(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player)
        {

            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self, player);
            }
            else if (self.twoDashes)
            {
                orig(self, player);
            }
            else if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS))
            {
                orig(self, player);
            }
        }
    }
}
