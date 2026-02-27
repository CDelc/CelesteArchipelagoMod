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
    internal class modCassetteBlock : IGameModification
    {

        private static Type CassetteZipMoverType;

        public override void Load()
        {
            CassetteZipMoverType = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.Entities.CassetteZipMover");
            On.Celeste.CassetteBlock.Update += modCassetteBlock_Update;
        }

        public override void Unload()
        {
            On.Celeste.CassetteBlock.Update -= modCassetteBlock_Update;
        }

        private static void modCassetteBlock_Update(On.Celeste.CassetteBlock.orig_Update orig, CassetteBlock self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            if (self.GetType() == CassetteZipMoverType && isBlue(self.color) && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_CASSETTE_TRAFFIC_BLOCK))
            {
                Disable(self);
            }
            else if (self.GetType() == CassetteZipMoverType && isPink(self.color) && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_CASSETTE_TRAFFIC_BLOCK))
            {
                Disable(self);
            }
            else if (self.GetType() == CassetteZipMoverType && isYellow(self.color) && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_CASSETTE_TRAFFIC_BLOCK))
            {
                Disable(self);
            }
            else if (self.GetType() != CassetteZipMoverType && isBlue(self.color) && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_CASSETTE))
            {
                Disable(self);
            }
            else if (self.GetType() != CassetteZipMoverType && isPink(self.color) && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_CASSETTE))
            {
                Disable(self);
            }
            else if (self.GetType() != CassetteZipMoverType && isYellow(self.color) && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_CASSETTE))
            {
                Disable(self);
            }
            else if (self.GetType() != CassetteZipMoverType && isGreen(self.color) && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_CASSETTE))
            {
                Disable(self);
            }
            else
            {
                orig(self);
            }
        }

        private static bool isBlue(Color color)
        {
            return color.R == 73 && color.G == 170 && color.B == 240;
        }

        private static bool isPink(Color color)
        {
            return color.R == 240 && color.G == 73 && color.B == 190;
        }

        private static bool isYellow(Color color)
        {
            return color.R == 252 && color.G == 220 && color.B == 58;
        }

        private static bool isGreen(Color color)
        {
            return color.R == 56 && color.G == 224 && color.B == 78;
        }

        private static void Disable(CassetteBlock self)
        {
            if (self.Activated)
            {
                self.ShiftSize(-1);
                self.SetActivatedSilently(false);
            }
        }
    }
}
