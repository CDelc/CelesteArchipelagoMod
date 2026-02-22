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
            if (!levelIDToSID.TryGetValue(levelID, out (string SID, AreaMode mode) rValue)) {
                throw new IndexOutOfRangeException($"A level SID was requested that does not exist: ID {levelID}");
            }
            return rValue;
        }

        public static string getRoomName(string SID, AreaMode mode, long roomID)
        {
            if (!roomIdsToname.TryGetValue((SID, mode), out Dictionary<long, string> roomDict))
            {
                throw new IndexOutOfRangeException($"A room name was requested in a level that does not exist {SID} {mode.ToString()}");
            }

            if (!roomDict.TryGetValue(roomID, out string rValue))
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
            if (!levelSIDToID.TryGetValue((SID, mode), out long rValue))
            {
                throw new IndexOutOfRangeException($"A level ID was requested that does not exist: ID {SID} | {mode.ToString()}");
            }
            return rValue;
        }

        public static long getRoomID(string SID, AreaMode mode, string room)
        {

            if (!roomNameToID.TryGetValue((SID, mode), out Dictionary<string, long> roomDict)) {
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
            long levelId = extractLevelID(id);
            if (levelIDToSID.TryGetValue(levelId, out (string SID, AreaMode mode) rValue))
            {
                return rValue;
            }

            throw new IndexOutOfRangeException($"A level SID was requested that does not exist: ID {id} | {levelId}");
        }

        private static LevelCategory getLevelCategory(string SID)
        {
            if (!levelSIDToCategory.TryGetValue(SID, out LevelCategory rValue))
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
            if (!(locationID >= 200000000000 && locationID < 300000000000 || locationID >= 900000000000 && locationID < 1000000000000 || locationID >= 1200000000000 && locationID < 1300000000000))
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

        public static long getCassetteLocationID(string SID, AreaMode mode)
        {
            long levelID = getLevelID(SID, mode);
            return 300000000000 + levelID * 100000000;
        }

        public static long getCheckpointItemID(string SID, AreaMode mode, string roomName)
        {
            try
            {
                return 300000000000 + getLocationOffset(SID, mode, roomName);
            } catch (Exception e)
            {
                Logger.Error(Constants.LOG_PREFIX, e.Message);
                return -1;
            }
        }

        public static long getCheckpointLocationID(string SID, AreaMode mode, string roomName)
        {
            return 700000000000 + getLocationOffset(SID, mode, roomName);
        }

        public static long getRoomLocationID(string SID, AreaMode mode, string room)
        {
            return 1300000000000 + getLocationOffset(SID, mode, room);
        }

        public static long getLevelCompleteLocationID(string SID, AreaMode mode)
        {
            return 400000000000 + getLevelID(SID, mode) * 100000000;
        }

        public static long getKeyLocationID(string SID, AreaMode mode, EntityID key)
        {
            return 800000000000 + getLocationOffset(SID, mode, key.Level) + key.ID;
        }

        public static long getLockDoorID(string SID, AreaMode mode, EntityID door)
        {
            return 500000000000 + getLocationOffset(SID, mode, door.Level) + door.ID;
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

        public static long getGemLocationID(string SID, AreaMode mode, EntityID gem)
        {
            return 1400000000000 + getLocationOffset(SID, mode, gem.Level) + gem.ID;
        }

        public static long getGemItemID(string SID, AreaMode mode, EntityID gem)
        {
            return 1100000000000 + getLocationOffset(SID, mode, gem.Level) + gem.ID;
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
                foreach (long heartID in CelesteArchipelagoModule.SaveData.CrystalHeartsCollab)
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
            {1, ("Celeste/1-ForsakenCity", AreaMode.Normal)},
            {2, ("Celeste/1-ForsakenCity", AreaMode.BSide)},
            {3, ("Celeste/1-ForsakenCity", AreaMode.CSide)},
            {4, ("Celeste/2-OldSite", AreaMode.Normal)},
            {5, ("Celeste/2-OldSite", AreaMode.BSide)},
            {6, ("Celeste/2-OldSite", AreaMode.CSide)},
            {19, ("Celeste/7-Summit", AreaMode.Normal) }
        };

        private static Dictionary<(string SID, AreaMode mode), long> levelSIDToID { get; } = levelIDToSID.ToDictionary(x => x.Value, x => x.Key);


        private static Dictionary<string, LevelCategory> levelSIDToCategory { get; } = new Dictionary<string, LevelCategory>
        {
            {"Celeste/1-ForsakenCity", LevelCategory.A_SIDE},
            {"Celeste/2-OldSite", LevelCategory.A_SIDE},
            {"Celeste/7-Summit", LevelCategory.A_SIDE}
        };

        private static Dictionary<LevelCategory, HashSet<string>> levelCategoryToSID { get; }
            = levelSIDToCategory.GroupBy(kvp => kvp.Value).ToDictionary(x => x.Key, x => x.Select(i => i.Key).ToHashSet());


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
            },
            {
                ("Celeste/1-ForsakenCity", AreaMode.BSide),
                new Dictionary<long, string>
                {
                    {0, "00"},
                    {1, "01"},
                    {2, "02"},
                    {3, "02b"},
                    {4, "03"},
                    {5, "04"},
                    {6, "05"},
                    {7, "05b"},
                    {8, "06"},
                    {9, "07"},
                    {10, "08"},
                    {11, "08b"},
                    {12, "09"},
                    {13, "10"},
                    {14, "11"},
                    {15, "end"}
                }
            },
            {
                ("Celeste/1-ForsakenCity", AreaMode.CSide),
                new Dictionary<long, string>
                {
                    {0, "00"},
                    {1, "01"},
                    {2, "02"}
                }
            },
            {
                ("Celeste/2-OldSite", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "start"},
                    {1, "s0"},
                    {2, "0"},
                    {3, "1"},
                    {4, "3x"},
                    {5, "3"},
                    {6, "4"},
                    {7, "5"},
                    {8, "6"},
                    {9, "7"},
                    {10, "8"},
                    {11, "9"},
                    {12, "9b"},
                    {13, "10"},
                    {14, "2"},
                    {15, "11"},
                    {16, "12b"},
                    {17, "12c"},
                    {18, "12d"},
                    {19, "12"},
                    {20, "13"},
                    {21, "end_0"},
                    {22, "end_1"},
                    {23, "end_2"},
                    {24, "end_3"},
                    {25, "end_4"},
                    {26, "end_3b"},
                    {27, "end_5"},
                    {28, "end_6"},
                    {30, "s1"},
                    {31, "s2"},
                    {32, "d0"},
                    {33, "d1"},
                    {34, "d6"},
                    {35, "d9"},
                    {36, "d7"},
                    {37, "d2"},
                    {38, "d4"},
                    {39, "d5"},
                    {40, "d8"},
                    {41, "d3"},
                    {42, "end_s0"},
                    {43, "end_s1"},
                    {44, "end_3cb"},
                    {45, "end_3c"}
                }
            },
            {
                ("Celeste/2-OldSite", AreaMode.BSide),
                new Dictionary<long, string>
                {
                    {0, "start"},
                    {1, "00"},
                    {2, "01"},
                    {3, "01b"},
                    {4, "02b"},
                    {5, "02"},
                    {6, "03"},
                    {7, "04"},
                    {8, "05"},
                    {9, "06"},
                    {10, "07"},
                    {11, "08b"},
                    {12, "08"},
                    {13, "10"},
                    {14, "11"},
                    {15, "end"}
                }
            },
            {
                ("Celeste/2-OldSite", AreaMode.CSide),
                new Dictionary<long, string>
                {
                    {0, "00"},
                    {1, "01"},
                    {2, "02"}
                }
            },
            {
                ("Celeste/7-Summit", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-00"},
                    {1, "a-01"},
                    {2, "a-02"},
                    {3, "a-02b"},
                    {4, "a-03"},
                    {5, "a-04"},
                    {6, "a-04b"},
                    {7, "a-05"},
                    {8, "a-06"},
                    {9, "b-00"},
                    {10, "b-01"},
                    {11, "b-02"},
                    {12, "b-02b"},
                    {13, "b-03"},
                    {14, "b-04"},
                    {15, "b-05"},
                    {16, "b-06"},
                    {17, "b-07"},
                    {18, "b-08"},
                    {19, "b-09"},
                    {20, "c-00"},
                    {21, "c-01"},
                    {22, "c-02"},
                    {23, "c-03"},
                    {24, "c-03b"},
                    {25, "c-04"},
                    {26, "c-05"},
                    {27, "c-06"},
                    {28, "c-06b"},
                    {29, "c-06c"},
                    {30, "c-07"},
                    {31, "c-07b"},
                    {32, "c-08"},
                    {33, "c-09"},
                    {34, "d-00"},
                    {35, "d-01"},
                    {36, "d-01b"},
                    {37, "d-01c"},
                    {38, "d-02"},
                    {39, "d-03"},
                    {40, "d-03b"},
                    {41, "d-04"},
                    {42, "d-05"},
                    {43, "d-05b"},
                    {44, "d-06"},
                    {45, "d-07"},
                    {46, "d-08"},
                    {47, "d-09"},
                    {48, "d-10"},
                    {49, "d-10b"},
                    {50, "d-11"},
                    {51, "e-00b"},
                    {52, "e-00"},
                    {53, "e-02"},
                    {54, "e-03"},
                    {55, "e-04"},
                    {56, "e-05"},
                    {57, "e-06"},
                    {58, "e-07"},
                    {59, "e-08"},
                    {60, "e-09"},
                    {61, "e-10"},
                    {62, "e-10b"},
                    {63, "e-13"},
                    {64, "f-00"},
                    {65, "f-01"},
                    {66, "f-02"},
                    {67, "f-02b"},
                    {68, "f-04"},
                    {69, "f-03"},
                    {70, "f-05"},
                    {71, "f-07"},
                    {72, "f-06"},
                    {73, "f-08"},
                    {74, "f-08b"},
                    {75, "f-09"},
                    {76, "f-10"},
                    {77, "f-10b"},
                    {78, "f-11"},
                    {79, "g-00"},
                    {80, "g-00b"},
                    {81, "g-01"},
                    {82, "g-02"},
                    {83, "g-03"},
                    {85, "b-02e"},
                    {86, "b-02c"},
                    {87, "b-02d"},
                    {88, "d-01d"},
                    {89, "e-01"},
                    {90, "e-01b"},
                    {91, "e-01c"},
                    {92, "e-11"},
                    {93, "e-12"},
                    {94, "f-08d"},
                    {95, "f-08c"}
                }
            }
        };

        private static Dictionary<(string SID, AreaMode mode), Dictionary<string, long>> roomNameToID =
            roomIdsToname.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Value, y => y.Key));



        public static bool mechanicEnabled(Mechanic mechanic)
        {
            return CelesteArchipelagoModule.SaveData.Mechanics.Contains(getMechanicID(mechanic));
        }
        private static long getMechanicID(Mechanic mechanic)
        {
            return 200000000000 + (int)mechanic;
        }

        public enum Mechanic
        {
            DASH_CRYSTALS,
            TRAFFIC_BLOCKS,
            SPRINGS,
            BLUE_CASSETTE,
            PINK_CASSETTE,
            CRUMBLING_PLATFORM,
            TOUCH_SWITCH,
            DREAM_BLOCK,
            BADELINE_ORB,
            SINKING_PLATFORM,
            GREEN_BUBBLES,
            CLOUDS,
            PINK_CLOUDS,
            MOVING_BLOCK,
            RED_BUBBLES,
            SWAP_BLOCK,
            DASH_SWITCH,
            FEATHER,
            MOVING_PLATFORM,
            WHITE_BLOCK
        }

        public static Dictionary<int, int> summitGemIndexMapping = new Dictionary<int, int>()
        {
            {110, 0},
            {109, 1},
            {333, 2},
            {449, 3},
            {8, 4},
            {679, 5}
        };

        public static Dictionary<int, EntityID> summitGemIndexReverseMapping = new Dictionary<int, EntityID>
        {
            {0, new EntityID("a-06", 110)},
            {1, new EntityID("b-02d", 109)},
            {2, new EntityID("c-06c", 333)},
            {3, new EntityID("d-05b", 449)},
            {4, new EntityID("e-01c", 8)},
            {5, new EntityID("f-02b", 679)}
        };
    }
}
