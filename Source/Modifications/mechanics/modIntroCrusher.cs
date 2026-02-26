using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modIntroCrusher : IGameModification
    {
        public override void Load()
        {
            On.Celeste.IntroCrusher.Added += modAdded;
        }

        public override void Unload()
        {
            On.Celeste.IntroCrusher.Added -= modAdded;
        }

        private void modAdded(On.Celeste.IntroCrusher.orig_Added orig, IntroCrusher self, Scene scene)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.INTRO_CRUSHER))
            {
                self.end = self.start;
                self.Position = self.start;
                self.tilegrid.Color = Color.Red;
            }
            orig(self, scene);
        }
    }
}
