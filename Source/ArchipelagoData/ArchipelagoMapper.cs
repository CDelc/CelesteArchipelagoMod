using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.CelesteArchipelago.ArchipelagoData
{
    internal class ArchipelagoMapper
    {

        public static long extractLevelID(long locationID)
        {
            long removeCategory = locationID % 100000000000;
            return removeCategory / 100000000;
        }

        public static long extractRoomID(long locationID)
        {
            long removeCategory = locationID % 100000000000;
            return (removeCategory % 100000000) / 100000;
        }

        public static int extractMetadata(long locationID)
        {
            long removeCategory = locationID % 100000000000;
            return (int)(removeCategory % 100000);
        }
            
        public static AreaModeStats getAreaModeStats(long levelID)
        {
            (string SID, AreaMode mode) = getSID(levelID);
            AreaData areaData = AreaData.Get(SID);
            if (areaData == null)
            {
                throw new ApplicationException($"Areadata not found for SID {SID}");
            }
            return SaveData.Instance.Areas_Safe[areaData.ID].Modes[(int)mode];
        }
        
        public static long getLocationOffset(string SID, AreaMode mode, string room)
        {
            return getLevelID(SID, mode) * 100000000 + getRoomID(SID, mode, room) * 100000;
        }

        public static (string SID, AreaMode mode) getSID(long levelID)
        {
            if(!levelIDToSID.TryGetValue(levelID, out (string SID, AreaMode mode) rValue)){
                throw new IndexOutOfRangeException($"A level SID was requested that does not exist: ID {levelID}");
            }
            return rValue;
        }

        public static string getRoomName(string SID, AreaMode mode, long roomID)
        {
            if(!roomIdsToname.TryGetValue((SID, mode), out Dictionary<long, string> roomDict))
            {
                throw new IndexOutOfRangeException($"A room name was requested in a level that does not exist {SID} {mode.ToString()}");
            }

            if(!roomDict.TryGetValue(roomID, out string rValue))
            {
                throw new IndexOutOfRangeException($"A room name was requested that does not exist in {SID} {mode.ToString()}: Room ID {roomID}");
            }

            return rValue;
        }

        public static string getRoomName(long levelID, long roomID)
        {
            (string SID, AreaMode mode) level = getSID(levelID);
            return getRoomName(level.SID, level.mode, roomID);
        }

        public static long getLevelID(string SID, AreaMode mode)
        {
            if(!levelSIDToID.TryGetValue((SID, mode), out long rValue))
            {
                throw new IndexOutOfRangeException($"A level ID was requested that does not exist: ID {SID} | {mode.ToString()}");
            }
            return rValue;
        }

        public static long getRoomID(string SID, AreaMode mode, string room)
        {

            if(!roomNameToID.TryGetValue((SID, mode), out Dictionary<string, long> roomDict)){
                throw new IndexOutOfRangeException($"A room ID was requested in a level that does not exist {SID} {mode.ToString()}");
            }

            if (!roomDict.TryGetValue(room, out long rValue))
            {
                throw new IndexOutOfRangeException($"A room ID was requested that does not exist in {SID} {mode.ToString()}: Room {room}");
            }

            return rValue;
        }

        public static (string SID, AreaMode mode) ArchipelagoIDToSID(long id)
        {
            long levelId = id - 400000000000;
            if (levelIDToSID.TryGetValue(levelId, out (string SID, AreaMode mode) rValue))
            {
                return rValue;
            }

            throw new IndexOutOfRangeException($"A level SID was requested that does not exist: ID {id} | {levelId}");
        }

        public static LevelCategory getLevelCategory(string SID)
        {
            if(!levelSIDToCategory.TryGetValue(SID, out LevelCategory rValue))
            {
                throw new IndexOutOfRangeException($"A level category was requested for a level that is not mapped: {SID}");
            }
            return rValue;
        }

        public static LevelCategory getLevelCategory(string SID, AreaMode mode)
        {
            if (mode == AreaMode.BSide)
            {
                return LevelCategory.B_SIDE;
            }
            if (mode == AreaMode.CSide)
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

        public static long getStrawberryLocationID(string SID, AreaMode mode, EntityID strawberryID, bool golden, bool winged)
        {
            return (golden ? winged ? 1300000000000 : 900000000000 : 200000000000) + getLocationOffset(SID, mode, strawberryID.Level) + strawberryID.ID;
        }

        public static EntityID getStrawberryEntityID(long locationID)
        {
            if(!(locationID >= 200000000000 && locationID < 300000000000 || locationID >= 900000000000 && locationID < 1000000000000 || locationID >= 1300000000000 && locationID < 1400000000000))
            {
                throw new IndexOutOfRangeException($"Strawberry was requested at locationID {locationID} but the ID is out of strawberry range");
            }

            long levelID = extractLevelID(locationID);
            long roomID = extractRoomID(locationID);

            string roomName = getRoomName(levelID, roomID);
            int entityID = extractMetadata(locationID);

            return new EntityID { Level = roomName, ID = entityID };
        }

        public static long getCrystalHeartLocationID(string SID, AreaMode mode)
        {
            long levelID = getLevelID(SID, mode);
            return 600000000000 + levelID * 100000000;
        }

        public static long getCheckpointItemID(string SID, AreaMode mode, string roomName)
        {
            long levelID = getLevelID(SID, mode);
            return 300000000000 + getLocationOffset(SID, mode, roomName);
        }

        public static long getCheckpointLocationID(string SID, AreaMode mode, string roomName)
        {
            long levelID = getLevelID(SID, mode);
            return 700000000000 + getLocationOffset(SID, mode, roomName);
        }

        public static long getRoomLocationID(string SID, AreaMode mode, string room)
        {
            return 1400000000000 + getLocationOffset(SID, mode, room);
        }

        public static long getMiniHeartLocationID(string SID, AreaMode mode)
        {
            long levelID = getLevelID(SID, mode);
            return 500000000000 + levelID * 100000000;
        }

        public static long getHeartLocationID(string SID, AreaMode mode)
        {
            LevelCategory category = getLevelCategory(SID, mode);
            if (isLobbyCategory(category))
            {
                return getMiniHeartLocationID(SID, mode);
            }
            return getCrystalHeartLocationID(SID, mode);
        }

        public static int getLobbyNumHeartsCollected(LevelCategory category)
        {
            if (!isLobbyCategory(category))
            {
                return CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Count;
            }
            else
            {
                int count = 0;
                foreach(long heartID in CelesteArchipelagoModule.SaveData.CrystalHeartsCollab)
                {
                    string SID = getSID(extractLevelID(heartID)).SID;
                    if (getLevelCategory(SID) == category)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public static bool isLobbyCategory(LevelCategory category)
        {
            return category == LevelCategory.BEGINNER
                || category == LevelCategory.INTERMEDIATE
                || category == LevelCategory.ADVANCED
                || category == LevelCategory.EXPERT
                || category == LevelCategory.GRANDMASTER
                || category == LevelCategory.CRACKED_GRANDMASTER;
        }

        public static bool isHeartsideCategory(LevelCategory category)
        {
            return category == LevelCategory.BEGINNER_HEARTSIDE
                || category == LevelCategory.INTERMEDIATE_HEARTSIDE
                || category == LevelCategory.ADVANCED_HEARTSIDE
                || category == LevelCategory.EXPERT_HEARTSIDE
                || category == LevelCategory.GRANDMASTER_HEARTSIDE;
        }

        private static Dictionary<long, (string SID, AreaMode mode)> levelIDToSID { get; } = new Dictionary<long, (string SID, AreaMode mode)>
        {
            {1, ("Celeste/1-ForsakenCity", AreaMode.Normal)}
        };

        private static Dictionary<(string SID, AreaMode mode), long> levelSIDToID { get; } = levelIDToSID.ToDictionary(x => x.Value, x => x.Key);


        private static Dictionary<string, LevelCategory> levelSIDToCategory { get; } = new Dictionary<string, LevelCategory>
        {
            {"Celeste/1-ForsakenCity", LevelCategory.A_SIDE}
        };


        private static Dictionary<(string SID, AreaMode mode), Dictionary<long, string>> roomIdsToname { get; } = new Dictionary<(string SID, AreaMode mode), Dictionary<long, string>>
        {
            {
                ("Celeste/1-ForsakenCity", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "3b"},
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

        private static Dictionary<(string SID, AreaMode mode), Dictionary<string, long>> roomNameToID =
            roomIdsToname.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Value, y => y.Key));



        public static bool mechanicEnabled(Mechanic mechanic)
        {
            return CelesteArchipelagoModule.SaveData.Mechanics[getMechanicID(mechanic)];
        }
        private static long getMechanicID(Mechanic mechanic)
        {
            return 200000000000 + (int)mechanic;
        }

        public enum Mechanic
        {
            CLIMB,
            DASH_CRYSTALS,
            TRAFFIC_BLOCKS,
            SPRINGS,
            ItemBLUE_CASSETTE,
            PINK_CASSETTE
        }
    }
}
