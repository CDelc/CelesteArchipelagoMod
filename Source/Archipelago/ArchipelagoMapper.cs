using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Archipelago
{
    internal class ArchipelagoMapper
    {

        public static KeyValuePair<string, AreaMode> ArchipelagoIDToSID(long id)
        {
            long levelId = id - 0x04000000;
            if (levelIDToSID.ContainsKey(levelId))
            {
                return new KeyValuePair<string, AreaMode>(levelIDToSID[levelId], levelIdToMode.ContainsKey(levelId) ? levelIdToMode[levelId] : AreaMode.Normal);
            }

            else throw new IndexOutOfRangeException($"A level SID was requested that does not exist: ID {id} | {levelId}");
        }

        public static LevelCategory getLevelCategory(string SID)
        {
            return levelSIDToCategory[SID];
        }

        public static LevelCategory getLevelCategory(string SID, AreaMode mode)
        {
            if(mode == AreaMode.BSide)
            {
                return LevelCategory.B_SIDE;
            }
            if(mode == AreaMode.CSide)
            {
                return LevelCategory.C_SIDE;
            }
            else
            {
                return getLevelCategory(SID);
            }
        }

        public static bool goldensEnabledOnCategory(LevelCategory levelCategory)
        {
            switch (levelCategory)
            {
                case LevelCategory.BEGINNER:
                    return ArchipelagoManager.Instance.include_beginner_silvers;
                case LevelCategory.INTERMEDIATE:
                    return ArchipelagoManager.Instance.include_intermediate_silvers;
                case LevelCategory.ADVANCED:
                    return ArchipelagoManager.Instance.include_advanced_silvers;
                case LevelCategory.EXPERT:
                    return ArchipelagoManager.Instance.include_expert_silvers;
                case LevelCategory.GRANDMASTER:
                    return ArchipelagoManager.Instance.include_grandmaster_silvers;
                case LevelCategory.CRACKED_GRANDMASTER:
                    return ArchipelagoManager.Instance.include_cracked_grandmaster_silvers;
                case LevelCategory.A_SIDE:
                    return ArchipelagoManager.Instance.include_a_sides_goldens;
                case LevelCategory.B_SIDE:
                    return ArchipelagoManager.Instance.include_b_sides_goldens;
                case LevelCategory.C_SIDE:
                    return ArchipelagoManager.Instance.include_c_sides_goldens;
                case LevelCategory.FAREWELL:
                    return ArchipelagoManager.Instance.include_farewell_golden;
                case LevelCategory.BEGINNER_HEARTSIDE:
                    return ArchipelagoManager.Instance.include_heart_side_golden && ArchipelagoManager.Instance.include_beginner_silvers;
                case LevelCategory.INTERMEDIATE_HEARTSIDE:
                    return ArchipelagoManager.Instance.include_heart_side_golden && ArchipelagoManager.Instance.include_intermediate_silvers;
                case LevelCategory.ADVANCED_HEARTSIDE:
                    return ArchipelagoManager.Instance.include_heart_side_golden && ArchipelagoManager.Instance.include_advanced_silvers;
                default:
                    return false;
            }
        }

        private static Dictionary<long, string> levelIDToSID { get; } = new Dictionary<long, string>
        {
            {1, "Celeste/1-ForsakenCity"}
        };

        private static Dictionary<long, AreaMode> levelIdToMode { get; } = new Dictionary<long, AreaMode>
        {

        };
        
        private static Dictionary<string, LevelCategory> levelSIDToCategory { get; } = new Dictionary<string, LevelCategory>
        {
               
        };


        public static Dictionary<string, Dictionary<long, string>> roomIdsToname { get; private set; } = new Dictionary<string, Dictionary<long, string>>
        {
            { 
                "Celeste/1-ForsakenCity",
                new Dictionary<long, string>
                {
                    {0, "5"},
                    {1, "1"},
                    {2, "2"},
                    {3, "3"},
                    {4, "4"},
                    {5, "5"},
                    {6, "6"},
                    {7, "7"},
                    {8, "8"},
                    {9, "9"},
                    {10, "10"},
                    {11, "11"},
                    {12, "12"},
                    {13, "5z"},
                    {14, "5a"},
                    {15, "6z"},
                    {16, "6zb"},
                    {17, "7zb"},
                    {18, "6a"},
                    {19, "6b"},
                    {20, "s0"},
                    {21, "s1"},
                    {22, "6c"},
                    {23, "7z"},
                    {24, "8z"},
                    {25, "8zb"},
                    {26, "9z"},
                    {27, "7a"},
                    {28, "8b"},
                    {30, "9b"},
                    {31, "10z"},
                    {32, "10zb"},
                    {34, "11z"},
                    {35, "9c"},
                    {36, "10a"},
                    {37, "12z"},
                    {38, "12a"},
                    {39, "end"}
                }
            }
        };

    }
}
