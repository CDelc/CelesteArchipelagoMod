using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modKevin : IGameModification
    {
        private static bool bNeedResetFace = false;

        public override void Load()
        {
            On.Celeste.CrushBlock.Render += modCrushBlock_Render;
            On.Celeste.CrushBlock.CanActivate += modCrushBlock_CanActivate;
        }

        public override void Unload()
        {
            On.Celeste.CrushBlock.Render -= modCrushBlock_Render;
            On.Celeste.CrushBlock.CanActivate -= modCrushBlock_CanActivate;
        }

        private static void modCrushBlock_Render(On.Celeste.CrushBlock.orig_Render orig, CrushBlock self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN))
            {
                self.face.Play("hurt", false, false);
                bNeedResetFace = true;
            }

            if (CelesteArchipelagoModule.shouldModMechanics && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN) && bNeedResetFace)
            {
                self.face.Play("idle", false, false);
                bNeedResetFace = false;
            }

            orig(self);
        }

        private static bool modCrushBlock_CanActivate(On.Celeste.CrushBlock.orig_CanActivate orig, CrushBlock self, Microsoft.Xna.Framework.Vector2 direction)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN))
            {
                return false;
            }
            else
            {
                return orig(self, direction);
            }
        }
    }
}
