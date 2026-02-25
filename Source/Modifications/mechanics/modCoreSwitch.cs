using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modCoreSwitch : IGameModification
    {
        public override void Load()
        {
            On.Celeste.CoreModeToggle.Update += modCoreModeToggle_Update;
        }

        public override void Unload()
        {
            On.Celeste.CoreModeToggle.Update -= modCoreModeToggle_Update;
        }

        private static void modCoreModeToggle_Update(On.Celeste.CoreModeToggle.orig_Update orig, CoreModeToggle self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CORE_SWITCH))
            {
                self.cooldownTimer = 1.6f;
                self.sprite.Play(self.iceMode ? "ice" : "hot", false, false);
            }

            orig(self);
        }
    }
}
