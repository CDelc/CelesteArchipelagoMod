using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modDreamBlock : IGameModification
    {
        public override void Load()
        {
            On.Celeste.DreamBlock.Render += modDreamBlock_Render;
            On.Celeste.DreamBlock.Update += modDreamBlock_Update;
            On.Celeste.Player.DreamDashCheck += modPlayer_DreamDashCheck;
        }

        public override void Unload()
        {
            On.Celeste.DreamBlock.Render -= modDreamBlock_Render;
            On.Celeste.DreamBlock.Update -= modDreamBlock_Update;
            On.Celeste.Player.DreamDashCheck -= modPlayer_DreamDashCheck;
        }

        private static void modDreamBlock_Render(On.Celeste.DreamBlock.orig_Render orig, DreamBlock self)
        {
            orig(self);

            if(!CelesteArchipelagoModule.shouldModMechanics)
            {
                return;
            }

            self.DisableLightsInside = !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK);
        }

        private static void modDreamBlock_Update(On.Celeste.DreamBlock.orig_Update orig, DreamBlock self)
        {
            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK) || !CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
            }
        }

        private static bool modPlayer_DreamDashCheck(On.Celeste.Player.orig_DreamDashCheck orig, Player self, Microsoft.Xna.Framework.Vector2 dir)
        {
            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK) || !CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, dir);
            }
            else
            {
                return false;
            }
        }
    }
}
