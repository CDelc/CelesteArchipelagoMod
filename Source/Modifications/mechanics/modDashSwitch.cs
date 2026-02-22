using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modDashSwitch : IGameModification
    {

        public override void Load()
        {
            On.Celeste.DashSwitch.Update += modDashSwitch_Update;
            On.Celeste.DashSwitch.OnDashed += modDashSwitch_OnDashed;
        }

        public override void Unload()
        {
            On.Celeste.DashSwitch.Update -= modDashSwitch_Update;
            On.Celeste.DashSwitch.OnDashed -= modDashSwitch_OnDashed;
        }

        private static void modDashSwitch_Update(On.Celeste.DashSwitch.orig_Update orig, DashSwitch self)
        {
            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_SWITCH) && CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                self.sprite.Play("idle", true);
            }

            orig(self);
        }

        private static DashCollisionResults modDashSwitch_OnDashed(On.Celeste.DashSwitch.orig_OnDashed orig, DashSwitch self, Player player, Microsoft.Xna.Framework.Vector2 direction)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_SWITCH))
            {
                return orig(self, player, direction);
            }
            return DashCollisionResults.NormalCollision;
        }

    }
}
