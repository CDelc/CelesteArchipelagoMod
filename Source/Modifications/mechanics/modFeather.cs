using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modFeather : IGameModification
    {

        public override void Load()
        {
            On.Celeste.FlyFeather.Render += modFlyFeather_Render;
            On.Celeste.FlyFeather.OnPlayer += modFlyFeather_OnPlayer;
        }

        public override void Unload()
        {
            On.Celeste.FlyFeather.Render -= modFlyFeather_Render;
            On.Celeste.FlyFeather.OnPlayer -= modFlyFeather_OnPlayer;
        }

        private static void modFlyFeather_Render(On.Celeste.FlyFeather.orig_Render orig, FlyFeather self)
        {
            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_SWITCH) && CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                self.sprite.Visible = false;
                self.Collidable = false;
                self.outline.Visible = true;
                self.respawnTimer = 1.0f;
            }

            orig(self);
        }

        private static void modFlyFeather_OnPlayer(On.Celeste.FlyFeather.orig_OnPlayer orig, FlyFeather self, Player player)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_SWITCH))
            {
                orig(self, player);
            }
        }

    }
}
