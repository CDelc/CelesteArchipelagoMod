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
        private static Type WonkyCassetteBlockType;
        private static Type CassetteMoveBlockType;
        private static Type CassetteSwapBlockType;

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
            if (!isActive(self))
            {
                Disable(self);
            }
            else
            {
                orig(self);
            }
        }

        private static bool isSpecialCassetteBlock(CassetteBlock self)
        {
            Type blockType = self.GetType();

            return blockType == CassetteZipMoverType || blockType == CassetteMoveBlockType || blockType == CassetteSwapBlockType;
        }

        private static bool isBlue(Color color)
        {
            return color.R == 73 && color.G == 170 && color.B == 240 || //Vanilla
                color.R == 74 && color.G == 143 && color.B == 231 || //Shattersong
                color.R == 0 && color.G == 136 && color.B == 221 || //GMHS Light
                color.R == 0 && color.G == 0 && color.B == 221 || //GMHS Dark
                //Synapse a3
                color.R == 71 && color.G == 108 && color.B == 106 ||
                //Synapse a4
                color.R == 77 && color.G == 100 && color.B == 121 ||
                color.R == 85 && color.G == 85 && color.B == 115 ||
                //Synapse b1_b
                color.R == 85 && color.G == 198 && color.B == 198 ||
                color.R == 85 && color.G == 85 && color.B == 198 ||
                //Synapse b3
                color.R == 77 && color.G == 100 && color.B == 121 ||
                //Synapse c1
                color.R == 70 && color.G == 109 && color.B == 102;
            
        }

        private static bool isPink(Color color)
        {
            return color.R == 240 && color.G == 73 && color.B == 190 || //Vanilla
                color.R == 202 && color.G == 46 && color.B == 85 || //Shattersong
                color.R == 221 && color.G == 0 && color.B == 221 || //GMHS
                //Synapse a5
                color.R == 118 && color.G == 26 && color.B == 89 ||
                color.R == 134 && color.G == 30 && color.B == 82 ||
                color.R == 153 && color.G == 35 && color.B == 73 ||
                //Synapse b1_b
                color.R == 198 && color.G == 85 && color.B == 198 ||
                //Synapse b3
                color.R == 111 && color.G == 38 && color.B == 95 ||
                color.R == 134 && color.G == 30 && color.B == 82 ||
                //Synapse c1
                color.R == 139 && color.G == 31 && color.B == 80;
        }

        private static bool isYellow(Color color)
        {
            return color.R == 252 && color.G == 220 && color.B == 58 || //Vanilla
                color.R == 255 && color.G == 140 && color.B == 66 || //Shattersong
                color.R == 221 && color.G == 170 && color.B == 0 || //GMHS
                //Synapse a2
                color.R == 200 && color.G == 138 && color.B == 17 ||
                color.R == 198 && color.G == 157 && color.B == 22 ||
                color.R == 196 && color.G == 175 && color.B == 27 ||
                color.R == 194 && color.G == 193 && color.B == 31 ||
                //Synapse b1
                color.R == 198 && color.G == 157 && color.B == 22 ||
                //Synapse b1_b
                color.R == 198 && color.G == 198 && color.B == 85 ||
                //Synapse b2
                color.R == 194 && color.G == 193 && color.B == 31 ||
                //Synapse c1
                color.R == 199 && color.G == 147 && color.B == 20 ||
                //AHS
                color.R == 199 && color.G == 147 && color.B == 20;
        }

        private static bool isGreen(Color color)
        {
            return color.R == 56 && color.G == 224 && color.B == 78 || //Vanilla
                color.R == 85 && color.G == 136 && color.B == 100 || //Shattersong
                color.R == 0 && color.G == 170 && color.B == 0 || //GMHS
                //Synapse a2
                color.R == 165 && color.G == 182 && color.B == 35 ||
                color.R == 134 && color.G == 169 && color.B == 38 ||
                //Synapse a3
                color.R == 66 && color.G == 115 && color.B == 91 ||
                color.R == 60 && color.G == 123 && color.B == 75 ||
                color.R == 103 && color.G == 157 && color.B == 42 ||
                color.R == 74 && color.G == 145 && color.B == 45 ||
                color.R == 50 && color.G == 135 && color.B == 48 ||
                color.R == 54 && color.G == 130 && color.B == 60 ||
                //Synapse b1_b
                color.R == 85 && color.G == 198 && color.B == 85 ||
                //Synapse b2
                color.R == 66 && color.G == 115 && color.B == 91 ||
                color.R == 134 && color.G == 169 && color.B == 38 ||
                color.R == 74 && color.G == 145 && color.B == 45 ||
                color.R == 54 && color.G == 130 && color.B == 60 ||
                //Synapse c1
                color.R == 50 && color.G == 135 && color.B == 48 ||
                color.R == 157 && color.G == 178 && color.B == 36;
        }

        private static bool isPurple(Color color)
        {
            return color.R == 136 && color.G == 0 && color.B == 221 || //GMHS
                //Synapse a4
                color.R == 94 && color.G == 69 && color.B == 108 ||
                //Synapse a5
                color.R == 103 && color.G == 53 && color.B == 101 ||
                color.R == 111 && color.G == 38 && color.B == 95 ||
                //Synapse b3
                color.R == 94 && color.G == 69 && color.B == 108 ||
                //Synapse c1
                color.R == 99 && color.G == 61 && color.B == 104;
        }

        private static bool isRed(Color color)
        {
            return color.R == 221 && color.G == 0 && color.B == 0 || //GMHS
                                                                     //Synapse a1
                color.R == 214 && color.G == 51 && color.B == 44 ||
                //Synapse a5
                color.R == 174 && color.G == 40 && color.B == 63 ||
                color.R == 195 && color.G == 46 && color.B == 53 ||
                //Synapse b1
                color.R == 214 && color.G == 51 && color.B == 44 ||
                //Synapse b1_b
                color.R == 198 && color.G == 85 && color.B == 85 ||
                //Synapse b3
                color.R == 174 && color.G == 40 && color.B == 63 ||
                //Synapse c1
                color.R == 214 && color.G == 51 && color.B == 44;
        }

        private static bool isOrange(Color color)
        {
            return color.R == 221 && color.G == 119 && color.B == 0 || //GMHS
                                                                       //Synapse a1
                color.R == 212 && color.G == 62 && color.B == 37 ||
                color.R == 210 && color.G == 74 && color.B == 29 ||
                color.R == 207 && color.G == 86 && color.B == 22 ||
                color.R == 205 && color.G == 97 && color.B == 15 ||
                color.R == 203 && color.G == 106 && color.B == 9 ||
                //Synapse a2
                color.R == 201 && color.G == 121 && color.B == 13 ||
                //Synapse b1
                color.R == 210 && color.G == 74 && color.B == 29 ||
                color.R == 205 && color.G == 97 && color.B == 15 ||
                color.R == 201 && color.G == 121 && color.B == 13 ||
                //Synapse c1
                color.R == 205 && color.G == 94 && color.B == 17;
        }

        private static bool isMappedColor(Color color)
        {
            return isBlue(color) ||
                isPink(color) ||
                isYellow(color) ||
                isGreen(color) ||
                isPurple(color) ||
                isRed(color) ||
                isOrange(color);
        }

        private static bool isActive(CassetteBlock self)
        {
            return self.GetType() == CassetteZipMoverType && isBlue(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_CASSETTE_TRAFFIC_BLOCK) ||
                self.GetType() == CassetteZipMoverType && isPink(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_CASSETTE_TRAFFIC_BLOCK) ||
                self.GetType() == CassetteZipMoverType && isYellow(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_CASSETTE_TRAFFIC_BLOCK) ||
                self.GetType() == CassetteZipMoverType && isGreen(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_CASSETTE_TRAFFIC_BLOCK) ||
                self.GetType() == CassetteZipMoverType && isOrange(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.ORANGE_CASSETTE_TRAFFIC_BLOCK) ||
                self.GetType() == CassetteZipMoverType && isRed(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_CASSETTE_TRAFFIC_BLOCK) ||
                !isSpecialCassetteBlock(self) && isBlue(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_CASSETTE) ||
                !isSpecialCassetteBlock(self) && isPink(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_CASSETTE) ||
                !isSpecialCassetteBlock(self) && isYellow(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_CASSETTE) ||
                !isSpecialCassetteBlock(self) && isGreen(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_CASSETTE) ||
                !isSpecialCassetteBlock(self) && isPurple(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_CASSETTE_BLOCK) ||
                !isSpecialCassetteBlock(self) && isRed(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_CASSETTE_BLOCK) ||
                !isSpecialCassetteBlock(self) && isOrange(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.ORANGE_CASSETTE_BLOCK) ||
                self.GetType() == CassetteMoveBlockType && isPurple(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_MOVING_CASSETTE_BLOCK) ||
                self.GetType() == CassetteMoveBlockType && isBlue(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_MOVING_CASSETTE_BLOCK) ||
                self.GetType() == CassetteMoveBlockType && isOrange(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.ORANGE_MOVING_CASSETTE_BLOCK) ||
                self.GetType() == CassetteMoveBlockType && isPink(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_MOVING_CASSETTE_BLOCK) ||
                self.GetType() == CassetteMoveBlockType && isGreen(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_MOVING_CASSETTE_BLOCK) ||
                self.GetType() == CassetteMoveBlockType && isYellow(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_MOVING_CASSETTE_BLOCK) ||
                self.GetType() == CassetteSwapBlockType && isPurple(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_CASSETTE_SWAP_BLOCK) ||
                self.GetType() == CassetteSwapBlockType && isPink(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_CASSETTE_SWAP_BLOCK) ||
                self.GetType() == CassetteSwapBlockType && isRed(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_CASSETTE_SWAP_BLOCK) ||
                self.GetType() == CassetteSwapBlockType && isGreen(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_CASSETTE_SWAP_BLOCK) ||
                self.GetType() == CassetteSwapBlockType && isYellow(self.color) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_CASSETTE_SWAP_BLOCK) ||
                !isMappedColor(self.color);
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
