using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.ArchipelagoData
{
    internal class ArchipelagoUtils
    {

        public static bool anyLevelIncludedInSet(string levelSet)
        {
            foreach(long levelID in ArchipelagoManager.Instance.included_levels)
            {
                string sid = ArchipelagoMapper.getSID(levelID).SID;
                if (sid.StartsWith(levelSet))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool anyLevelIncludedInCategory(LevelCategory levelCategory)
        {
            if (ArchipelagoManager.Instance == null) return true;
            foreach (long levelID in ArchipelagoManager.Instance.included_levels)
            {
                string sid = ArchipelagoMapper.getSID(levelID).SID;
                LevelCategory category = ArchipelagoMapper.getLevelCategory(sid);
                if (category == levelCategory) return true;
            }
            return false;
        }

        public static void setUnlockedAreas()
        {
            if (SaveData.Instance == null) return;
            foreach (LevelSetStats levelSet in SaveData.Instance.LevelSets)
            {
                if (levelSet.Name == "StrawberryJam2021/0-Lobbies")
                {
                    if (ArchipelagoUtils.anyLevelIncludedInCategory(LevelCategory.GRANDMASTER))
                    {
                        levelSet.UnlockedAreas = 5;
                    }
                    else if (ArchipelagoUtils.anyLevelIncludedInCategory(LevelCategory.EXPERT))
                    {
                        levelSet.UnlockedAreas = 4;
                    }
                    else if (ArchipelagoUtils.anyLevelIncludedInCategory(LevelCategory.ADVANCED))
                    {
                        levelSet.UnlockedAreas = 3;
                    }
                    else if (ArchipelagoUtils.anyLevelIncludedInCategory(LevelCategory.INTERMEDIATE))
                    {
                        levelSet.UnlockedAreas = 2;
                    }
                    else if (ArchipelagoUtils.anyLevelIncludedInCategory(LevelCategory.BEGINNER))
                    {
                        levelSet.UnlockedAreas = 1;
                    }
                    else
                    {
                        levelSet.UnlockedAreas = 0;
                    }
                }
                else if (levelSet.Name == "Celeste")
                {
                    SaveData.Instance.UnlockedAreas = 10;
                    if (!ArchipelagoUtils.anyLevelIncludedInCategory(LevelCategory.FAREWELL))
                    {
                        SaveData.Instance.UnlockedAreas = 9;
                    }
                }
            }
        }
        
        // Checks if a specific level should be considered unlocked by default
        public static bool levelStartsUnlocked(string sid, AreaMode areaMode)
        {
            return ArchipelagoMapper.levelIsAllowed(sid) && (isLobbyOrGymSID(sid) || ArchipelagoManager.Instance.preunlocked_levels.Contains((long)ArchipelagoMapper.getLevelID(sid, areaMode)));
        }

        //Checks if a level is included in this randomizer at all
        public static bool levelIncludedInRandomizer(string sid, AreaMode areaMode)
        {
            if (isLobbyOrGymSID(sid)) return true;
            long? levelID = ArchipelagoMapper.getLevelID(sid, areaMode);
            if (levelID == null) return false;
            if (isGoalLevel(sid, areaMode))
            {
                return true;
            }
            return ArchipelagoManager.Instance.included_levels.Contains(levelID.Value);
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

        public static Dictionary<AreaMode, long?> getAllModeLevelIDs(string sid)
        {
            Dictionary<AreaMode, long?> rValue = new Dictionary<AreaMode, long?>();
            rValue[AreaMode.Normal] = ArchipelagoMapper.getLevelID(sid, AreaMode.Normal);
            rValue[AreaMode.BSide] = ArchipelagoMapper.getLevelID(sid, AreaMode.BSide);
            rValue[AreaMode.CSide] = ArchipelagoMapper.getLevelID(sid, AreaMode.CSide);
            return rValue;
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
            return sid != null && (sid.Contains("StrawberryJam2021/0-Lobbies") || sid.Contains("StrawberryJam2021/0-Gyms")) && !sid.Contains("0-Lobbies/0-Prologue");
        }

        public static bool isLobbySID(string sid)
        {
            return sid != null && sid.Contains("StrawberryJam2021/0-Lobbies") && !sid.Contains("0-Prologue");
        }

        public static bool isGymSID(string sid)
        {
            return sid != null && sid.Contains("StrawberryJam2021/0-Gyms");
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
            long levelID = (long)ArchipelagoMapper.getLevelID(SID, mode);
            return ArchipelagoManager.Instance.deathless_levels.Contains(levelID);
        }

        // Checks if room checks should be enabled for a specific level category
        public static bool roomChecksEnabled(string SID, AreaMode mode)
        {
            long levelID = (long)ArchipelagoMapper.getLevelID(SID, mode);
            return ArchipelagoManager.Instance.roomcheck_levels.Contains(levelID);
        }

        public static bool allLocationsChecked(string SID, AreaMode mode)
        {
            long levelID = (long)ArchipelagoMapper.getLevelID(SID, mode);
            List<long> location_list = ArchipelagoManager.Instance.allLocationByLevelID.TryGetValue(levelID, out var locations) ? locations : null;
            if (location_list == null) return true;

            foreach(long locID in location_list)
            {
                if (!CelesteArchipelagoModule.SaveData.LocationsChecked.Contains(locID)) return false;
            }
            return true;
        }

        public static bool checkAccessRule(List<List<long>> access_rule)
        {
            bool result = true;
            foreach (List<long> andItems in access_rule)
            {
                result = true;
                foreach (long itemID in andItems)
                {
                    // Crystal Heart number requirement
                    if (itemID < 0)
                    {
                        result = CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Count >= Math.Abs(itemID);
                    }
                    else
                    {
                        result = CelesteArchipelagoModule.SaveData.UniqueItemsCollected.Contains(itemID);
                    }
                    if (!result) break;
                }
                if (!result) continue;
                else return true;
            }
            return result;
        }
    }
}
