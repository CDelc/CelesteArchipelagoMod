using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modTrafficBlocks : IGameModification
    {

        static Color originalColor;
        static bool setColor = false;

        public override void Load()
        {
            On.Celeste.ZipMover.Render += modZipMover_Render;
            On.Celeste.ZipMover.Update += modZipMover_Update;
        }

        public override void Unload()
        {
            On.Celeste.ZipMover.Render -= modZipMover_Render;
            On.Celeste.ZipMover.Update -= modZipMover_Update;
        }

        private void modZipMover_Render(On.Celeste.ZipMover.orig_Render orig, ZipMover self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                return;
            }

            if (!setColor)
            {
                originalColor = self.streetlight.Color;
                setColor = true;
            }

            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TRAFFIC_BLOCKS))
            {
                self.streetlight.Color = Color.DarkRed;
            }
            else
            {
                self.streetlight.Color = originalColor;
            }
        }

        private void modZipMover_Update(On.Celeste.ZipMover.orig_Update orig, ZipMover self)
        {
            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TRAFFIC_BLOCKS) || !CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self);
            }
        }
    }
}
