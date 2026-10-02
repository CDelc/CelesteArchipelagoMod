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
            return ArchipelagoMapper.levelIsAllowed(sid) && (isLobbyOrGymSID(sid) || ArchipelagoManager.Instance.preunlocked_levels.Contains(ArchipelagoMapper.getLevelID(sid, areaMode)));
        }

        //Checks if a level is included in this randomizer at all
        public static bool levelIncludedInRandomizer(string sid, AreaMode areaMode)
        {
            if (isLobbyOrGymSID(sid)) return true;
            long levelID = ArchipelagoMapper.getLevelID(sid, areaMode);
            if (isGoalLevel(sid, areaMode))
            {
                return true;
            }
            return ArchipelagoManager.Instance.included_levels.Contains(levelID);
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
                || category == LevelCategory.GRANDMASTER;
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

        // Checks if golden/silver berries should be enabled in a given level
        public static bool deathlessEnabled(string SID, AreaMode mode)
        {
            long levelID = ArchipelagoMapper.getLevelID(SID, mode);
            return ArchipelagoManager.Instance.deathless_levels.Contains(levelID);
        }

        // Checks if room checks should be enabled for a specific level category
        public static bool roomChecksEnabled(string SID, AreaMode mode)
        {
            long levelID = ArchipelagoMapper.getLevelID(SID, mode);
            return ArchipelagoManager.Instance.roomcheck_levels.Contains(levelID);
        }
    }
}
