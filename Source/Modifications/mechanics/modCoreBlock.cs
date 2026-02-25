using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modCoreBlock : IGameModification
    {
        public override void Load()
        {
            On.Celeste.BounceBlock.WindUpPlayerCheck += modBounceBlock_WindUpPlayerCheck;
            On.Celeste.BounceBlock.Render += modBounceBlock_Render;
        }

        public override void Unload()
        {
            On.Celeste.BounceBlock.WindUpPlayerCheck -= modBounceBlock_WindUpPlayerCheck;
            On.Celeste.BounceBlock.Render -= modBounceBlock_Render;
        }

        private static Player modBounceBlock_WindUpPlayerCheck(On.Celeste.BounceBlock.orig_WindUpPlayerCheck orig, BounceBlock self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CORE_BLOCK))
            {
                return orig(self);
            }

            Player res = orig(self);

            if (res != null)
            {
                self.Break();
            }

            return null;
        }

        private static void modBounceBlock_Render(On.Celeste.BounceBlock.orig_Render orig, BounceBlock self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CORE_BLOCK))
            {
                self.hotCenterSprite.Visible = false;
                self.coldCenterSprite.Visible = false;
            }
            else if (CelesteArchipelagoModule.shouldModMechanics)
            {
                self.ToggleSprite();
            }

            orig(self);
        }
    }
}
