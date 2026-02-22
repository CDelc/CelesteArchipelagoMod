using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modBadelineBooster : IGameModification
    {
        public override void Load()
        {
            On.Celeste.BadelineBoost.OnPlayer += modBadelineBoost_OnPlayer;
            On.Celeste.BadelineBoost.Update += modBadelineBoost_Update;
        }

        public override void Unload()
        {
            On.Celeste.BadelineBoost.OnPlayer -= modBadelineBoost_OnPlayer;
            On.Celeste.BadelineBoost.Update -= modBadelineBoost_Update;
        }

        private static void modBadelineBoost_OnPlayer(On.Celeste.BadelineBoost.orig_OnPlayer orig, BadelineBoost self, Player player)
        {
            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BADELINE_ORB) || !CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self, player);
            }
        }

        private static void modBadelineBoost_Update(On.Celeste.BadelineBoost.orig_Update orig, BadelineBoost self)
        {
            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BADELINE_ORB) && CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                self.sprite.Color = Microsoft.Xna.Framework.Color.DarkRed;
            }
            else
            {
                self.sprite.Color = Microsoft.Xna.Framework.Color.White;
            }

            orig(self);
        }
    }
}
