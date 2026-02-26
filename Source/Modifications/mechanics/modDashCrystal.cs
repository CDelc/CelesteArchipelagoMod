using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
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

        private static void modRefill_Render(On.Celeste.Refill.orig_Render orig, Refill self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            bool isDreamRefill = false;
            if(self.GetType() == CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.DashStates.DreamTunnelRefill"))
            {
                isDreamRefill = true;
            }
            if (!isDreamRefill && self.twoDashes && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_CRYSTAL))
            {
                self.outline.Visible = true;
                self.sprite.Visible = false;
                self.Collidable = false;
                self.respawnTimer = 2.5f;
            }
            else if (!isDreamRefill && !self.twoDashes && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS))
            {
                self.outline.Visible = true;
                self.sprite.Visible = false;
                self.Collidable = false;
                self.respawnTimer = 2.5f;
            }
            else if (isDreamRefill && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_DASH_CRYSTALS))
            {
                self.outline.Visible = true;
                self.sprite.Visible = false;
                self.Collidable = false;
                self.respawnTimer = 2.5f;
            }
            orig(self);
        }

        private static void modRefill_OnPlayer(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player)
        {

            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self, player);
                return;
            }

            if (self.GetType() == CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.DashStates.DreamTunnelRefill"))
            {
                orig(self, player);
            }

            if (self.twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_CRYSTAL))
            {
                orig(self, player);
            }
            else if (!self.twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS))
            {
                orig(self, player);
            }
        }
    }
}
