using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.ArchipelagoData
{
    internal class ArchipelagoUtils
    {

        // Checks if a specific level should be considered unlocked by default
        public static bool levelStartsUnlocked(string sid, AreaMode areaMode)
        {
            return isLobbyOrGymSID(sid) ||
                ArchipelagoMapper.getLevelCategory(sid, areaMode) == ArchipelagoManager.Instance.starting_category ||
                ArchipelagoManager.Instance.starting_category == LevelCategory.ALL ||
                (ArchipelagoManager.Instance.starting_level_sid == sid && areaMode == AreaMode.Normal) ||
                (ArchipelagoManager.Instance.heart_sides_start_unlocked && isHeartsideSID(sid));
        }

        //Checks if a level is included in this randomizer at all
        public static bool levelIncludedInRandomizer(string sid, AreaMode areaMode)
        {
            if (isLobbyOrGymSID(sid)) return true;
            long levelID = ArchipelagoMapper.getLevelID(sid, areaMode);
            if(levelID == 142)
            {
                return ArchipelagoManager.Instance.include_grandmaster && ArchipelagoManager.Instance.include_cracked_grandmaster;
            }
            else if(ArchipelagoMapper.isPuzzleLevel(sid))
            {
                return false;
            }
            else if(ArchipelagoManager.Instance.starting_level_sid == sid && areaMode == AreaMode.Normal)
            {
                return true;
            }
            else
            {
                return levelsEnabledOnCategory(ArchipelagoMapper.getLevelCategory(sid, areaMode));
            }
        }

        // Checks if a level should be unlocked
        public static bool levelIsUnlocked(string sid, AreaMode areaMode)
        {
            return levelStartsUnlocked(sid, areaMode) || CelesteArchipelagoModule.SaveData.LevelUnlocks.Contains((sid, areaMode));
        }

        // Checks whether the player is still missing berries before achieving the berry goal
        public static bool berriesMissing()
        {
            return CelesteArchipelagoModule.SaveData.Strawberries < ArchipelagoManager.Instance.required_strawberries || (ArchipelagoManager.Instance.require_moon_berry && !CelesteArchipelagoModule.SaveData.moonBerryCollected);
        }

        // Checks if the given level is the goal level
        public static bool isGoalLevel(string sid, AreaMode areaMode)
        {
            return ArchipelagoManager.Instance.win_condition_level == (sid, areaMode);
        }

        // Checks if the player should be able to enter a level
        public static bool canEnter(string sid, AreaMode areaMode)
        {
            return ArchipelagoMapper.levelIsAllowed(sid) &&
                levelIsUnlocked(sid, areaMode) &&
                levelIncludedInRandomizer(sid, areaMode) &&
                !(isGoalLevel(sid, areaMode) && berriesMissing() && ArchipelagoManager.Instance.require_berries_for_goal);
        }

        // Checks if a level is included in the current randomizer
        public static bool isIncludedInRandomizer(string sid, AreaMode areaMode)
        {
            return ArchipelagoMapper.levelIsAllowed(sid) && ArchipelagoUtils.levelsEnabledOnCategory(ArchipelagoMapper.getLevelCategory(sid, areaMode));
        }

        public static bool levelHasBCSides(string sid)
        {
            return sid.StartsWith("Celeste/") && !sid.Contains("LostLevels") && !sid.Contains("0-Intro") && !sid.Contains("8-Epilogue");
        }

        // Checks if a level category is part of a collab lobby
        public static bool isLobbyCategory(LevelCategory category)
        {
            return category == LevelCategory.BEGINNER
                || category == LevelCategory.INTERMEDIATE
                || category == LevelCategory.ADVANCED
                || category == LevelCategory.EXPERT
                || category == LevelCategory.GRANDMASTER
                || category == LevelCategory.CRACKED_GRANDMASTER;
        }

        // Checks if an SID is a lobby or gym level
        public static bool isLobbyOrGymSID(string sid)
        {
            return sid.Contains("StrawberryJam2021/0-Lobbies") || sid.Contains("StrawberryJam2021/0-Gyms");
        }

        // Checks if an SID is a heartside level
        public static bool isHeartsideSID(string sid)
        {
            return sid.Contains("/ZZ-HeartSide");
        }

        // Gets the number of hearts collected for a specific lobby's heart gate (Or vanilla heart gates)
        public static int getLobbyNumHeartsCollected(LevelCategory category)
        {
            if (!isLobbyCategory(category))
            {
                return CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Count;
            }
            else
            {
                int count = 0;
                foreach (long heartID in CelesteArchipelagoModule.SaveData.CrystalHeartsCollab)
                {
                    string SID = ArchipelagoMapper.getSID(ArchipelagoMapper.extractLevelID(heartID)).SID;
                    if (ArchipelagoMapper.getLevelCategory(SID) == category)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        // Checks if golden/silver berries should be enabled in a given level category
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
                default:
                    return false;
            }
        }

        // Checks if the levels in the given category are enabled in this randomizer
        public static bool levelsEnabledOnCategory(LevelCategory levelCategory)
        {
            switch (levelCategory)
            {
                case LevelCategory.BEGINNER:
                    return ArchipelagoManager.Instance.include_beginner;
                case LevelCategory.INTERMEDIATE:
                    return ArchipelagoManager.Instance.include_intermediate;
                case LevelCategory.ADVANCED:
                    return ArchipelagoManager.Instance.include_advanced;
                case LevelCategory.EXPERT:
                    return ArchipelagoManager.Instance.include_expert;
                case LevelCategory.GRANDMASTER:
                    return ArchipelagoManager.Instance.include_grandmaster;
                case LevelCategory.CRACKED_GRANDMASTER:
                    return ArchipelagoManager.Instance.include_cracked_grandmaster;
                case LevelCategory.A_SIDE:
                    return ArchipelagoManager.Instance.include_a_sides;
                case LevelCategory.B_SIDE:
                    return ArchipelagoManager.Instance.include_b_sides;
                case LevelCategory.C_SIDE:
                    return ArchipelagoManager.Instance.include_c_sides;
                case LevelCategory.FAREWELL:
                    return ArchipelagoManager.Instance.include_farewell;
                default:
                    return false;
            }
        }
    }
}
