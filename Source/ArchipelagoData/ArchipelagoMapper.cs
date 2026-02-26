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
            foreach (KeyValuePair<string, LevelCategory> kvp in levelSIDToCategory)
            {
                if (SID.StartsWith(kvp.Key))
                {
                    return kvp.Value;
                }
            }
            return LevelCategory.A_SIDE;
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
            {7, ("Celeste/3-CelestialResort", AreaMode.Normal)},
            {8, ("Celeste/3-CelestialResort", AreaMode.BSide)},
            {9, ("Celeste/3-CelestialResort", AreaMode.CSide)},
            {10, ("Celeste/4-GoldenRidge", AreaMode.Normal)},
            {11, ("Celeste/4-GoldenRidge", AreaMode.BSide)},
            {12, ("Celeste/4-GoldenRidge", AreaMode.CSide)},
            {13, ("Celeste/5-MirrorTemple", AreaMode.Normal)},
            {14, ("Celeste/5-MirrorTemple", AreaMode.BSide)},
            {15, ("Celeste/5-MirrorTemple", AreaMode.CSide)},
            {16, ("Celeste/6-Reflection", AreaMode.Normal)},
            {17, ("Celeste/6-Reflection", AreaMode.BSide)},
            {18, ("Celeste/6-Reflection", AreaMode.CSide)},
            {19, ("Celeste/7-Summit", AreaMode.Normal)},
            {20, ("Celeste/7-Summit", AreaMode.BSide)},
            {21, ("Celeste/7-Summit", AreaMode.CSide)},
            {22, ("Celeste/9-Core", AreaMode.Normal)},
            {23, ("Celeste/9-Core", AreaMode.BSide)},
            {24, ("Celeste/9-Core", AreaMode.CSide)},
            {25, ("Celeste/LostLevels", AreaMode.Normal)},
            {26, ("Celeste/0-Intro", AreaMode.Normal)},
            {27, ("StrawberryJam2021/1-Beginner/coffe", AreaMode.Normal)},
            {28, ("StrawberryJam2021/1-Beginner/asteriskblue", AreaMode.Normal)},
            {29, ("StrawberryJam2021/1-Beginner/Bing_Over_Google", AreaMode.Normal)},
            {30, ("StrawberryJam2021/1-Beginner/cellularAutomaton", AreaMode.Normal)},
            {999, ("StrawberryJam2021/0-Lobbies/1-Beginner", AreaMode.Normal)},
            {998, ("StrawberryJam2021/0-Lobbies/2-Intermediate", AreaMode.Normal)},
            {997, ("StrawberryJam2021/0-Lobbies/3-Advanced", AreaMode.Normal)},
            {996, ("StrawberryJam2021/0-Lobbies/4-Expert", AreaMode.Normal)},
            {995, ("StrawberryJam2021/0-Lobbies/5-Grandmaster", AreaMode.Normal)}
        };

        public static Dictionary<(string SID, AreaMode mode), long> levelSIDToID { get; } = levelIDToSID.ToDictionary(x => x.Value, x => x.Key);


        private static Dictionary<string, LevelCategory> levelSIDToCategory { get; } = new Dictionary<string, LevelCategory>
        {
            {"Celeste/1-ForsakenCity", LevelCategory.A_SIDE},
            {"Celeste/2-OldSite", LevelCategory.A_SIDE},
            {"Celeste/3-CelestialResort", LevelCategory.A_SIDE},
            {"Celeste/4-GoldenRidge", LevelCategory.A_SIDE},
            {"Celeste/5-MirrorTemple", LevelCategory.A_SIDE},
            {"Celeste/6-Reflection", LevelCategory.A_SIDE},
            {"Celeste/7-Summit", LevelCategory.A_SIDE},
            {"Celeste/9-Core", LevelCategory.A_SIDE},
            {"Celeste/LostLevels", LevelCategory.FAREWELL},
            {"Celeste/0-Intro", LevelCategory.A_SIDE},
            {"StrawberryJam2021/1-Beginner/", LevelCategory.BEGINNER}
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
                ("Celeste/3-CelestialResort", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "s0"},
                    {1, "s1"},
                    {2, "s2"},
                    {3, "s3"},
                    {4, "0x-a"},
                    {5, "00a"},
                    {6, "02-a"},
                    {7, "02-b"},
                    {8, "03-a"},
                    {9, "04-b"},
                    {10, "05-a"},
                    {11, "06-a"},
                    {12, "07-a"},
                    {13, "08-a"},
                    {14, "08-x"},
                    {15, "09-b"},
                    {16, "09-d"},
                    {17, "08-d"},
                    {18, "06-d"},
                    {19, "04-d"},
                    {20, "04-c"},
                    {21, "02-d"},
                    {22, "00-d"},
                    {23, "roof00"},
                    {24, "roof01"},
                    {25, "roof02"},
                    {26, "roof03"},
                    {27, "roof04"},
                    {28, "roof05"},
                    {29, "roof06b"},
                    {30, "roof06"},
                    {31, "roof07"},
                    {33, "01-b"},
                    {34, "00-b"},
                    {35, "00-c"},
                    {36, "0x-b"},
                    {37, "07-b"},
                    {38, "06-b"},
                    {39, "06-c"},
                    {40, "05-c"},
                    {41, "08-c"},
                    {42, "08-b"},
                    {43, "10-x"},
                    {44, "11-x"},
                    {45, "11-y"},
                    {46, "12-y"},
                    {47, "11-z"},
                    {48, "10-z"},
                    {49, "10-y"},
                    {50, "11-b"},
                    {51, "12-b"},
                    {52, "13-b"},
                    {53, "13-a"},
                    {54, "13-x"},
                    {55, "12-x"},
                    {56, "11-a"},
                    {57, "10-c"},
                    {58, "11-c"},
                    {59, "12-c"},
                    {60, "12-d"},
                    {61, "11-d"},
                    {62, "10-d"},
                    {63, "03-b"},
                    {64, "01-c"},
                    {66, "02-c"}
                }
            },
            {
                ("Celeste/3-CelestialResort", AreaMode.BSide),
                new Dictionary<long, string>
                {
                    {0, "00"},
                    {1, "back"},
                    {2, "01"},
                    {3, "02"},
                    {4, "03"},
                    {5, "04"},
                    {6, "05"},
                    {7, "06"},
                    {8, "07"},
                    {9, "08"},
                    {10, "09"},
                    {11, "10"},
                    {12, "11"},
                    {13, "13"},
                    {14, "14"},
                    {15, "15"},
                    {16, "12"},
                    {17, "16"},
                    {18, "17"},
                    {19, "18"},
                    {20, "19"},
                    {21, "21"},
                    {22, "20"},
                    {23, "end"}
                }
            },
            {
                ("Celeste/3-CelestialResort", AreaMode.CSide),
                new Dictionary<long, string>
                {
                    {0, "00"},
                    {1, "01"},
                    {2, "02"}
                }
            },
            {
                ("Celeste/4-GoldenRidge", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-00"},
                    {1, "a-01"},
                    {2, "a-01x"},
                    {3, "a-02"},
                    {4, "a-03"},
                    {5, "a-04"},
                    {6, "a-05"},
                    {7, "a-06"},
                    {8, "a-07"},
                    {9, "a-08"},
                    {10, "a-10"},
                    {11, "a-09"},
                    {12, "b-00"},
                    {13, "b-01"},
                    {14, "b-03"},
                    {15, "b-04"},
                    {16, "b-02"},
                    {17, "b-sec"},
                    {18, "b-05"},
                    {19, "b-08b"},
                    {20, "b-08"},
                    {21, "c-00"},
                    {22, "c-01"},
                    {23, "c-02"},
                    {24, "c-04"},
                    {25, "c-05"},
                    {26, "c-06"},
                    {27, "c-06b"},
                    {28, "c-09"},
                    {29, "c-07"},
                    {30, "c-08"},
                    {31, "c-10"},
                    {32, "d-00"},
                    {33, "d-00b"},
                    {34, "d-01"},
                    {35, "d-02"},
                    {36, "d-03"},
                    {37, "d-04"},
                    {38, "d-05"},
                    {39, "d-06"},
                    {40, "d-07"},
                    {41, "d-08"},
                    {42, "d-09"},
                    {43, "d-10"},
                    {45, "a-11"},
                    {46, "b-06"},
                    {47, "b-07"},
                    {48, "b-secb"}
                }
            },
            {
                ("Celeste/4-GoldenRidge", AreaMode.BSide),
                new Dictionary<long, string>
                {
                    {0, "a-00"},
                    {1, "a-01"},
                    {2, "a-02"},
                    {3, "a-03"},
                    {4, "a-04"},
                    {5, "b-00"},
                    {6, "b-01"},
                    {7, "b-02"},
                    {8, "b-03"},
                    {9, "b-04"},
                    {10, "c-00"},
                    {11, "c-01"},
                    {12, "c-02"},
                    {13, "c-03"},
                    {14, "c-04"},
                    {15, "d-00"},
                    {16, "d-01"},
                    {17, "d-02"},
                    {18, "d-03"},
                    {19, "end"}
                }
            },
            {
                ("Celeste/4-GoldenRidge", AreaMode.CSide),
                new Dictionary<long, string>
                {
                    {0, "00"},
                    {1, "01"},
                    {2, "02"}
                }
            },
            {
                ("Celeste/5-MirrorTemple", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-00x"},
                    {1, "a-00b"},
                    {2, "a-00d"},
                    {3, "a-00c"},
                    {4, "a-00"},
                    {5, "a-01"},
                    {6, "a-04"},
                    {7, "a-02"},
                    {8, "a-08"},
                    {9, "a-13"},
                    {10, "b-00"},
                    {11, "b-18"},
                    {12, "b-01"},
                    {13, "b-20"},
                    {14, "b-01c"},
                    {15, "b-01b"},
                    {16, "b-02"},
                    {17, "b-03"},
                    {18, "b-05"},
                    {19, "b-04"},
                    {20, "b-10"},
                    {21, "b-11"},
                    {22, "b-06"},
                    {23, "b-19"},
                    {24, "b-14"},
                    {25, "b-15"},
                    {26, "b-16"},
                    {27, "c-00"},
                    {28, "c-01"},
                    {29, "c-01b"},
                    {30, "c-01c"},
                    {31, "c-08b"},
                    {32, "c-08"},
                    {33, "c-10"},
                    {34, "c-12"},
                    {35, "c-07"},
                    {36, "c-11"},
                    {37, "c-09"},
                    {38, "c-13"},
                    {39, "d-00"},
                    {40, "d-01"},
                    {41, "d-09"},
                    {42, "d-04"},
                    {43, "d-19"},
                    {44, "d-19b"},
                    {45, "d-10"},
                    {46, "d-20"},
                    {47, "e-00"},
                    {48, "e-01"},
                    {49, "e-02"},
                    {50, "e-03"},
                    {51, "e-04"},
                    {52, "e-06"},
                    {53, "e-05"},
                    {54, "e-07"},
                    {55, "e-08"},
                    {56, "e-09"},
                    {57, "e-10"},
                    {58, "e-11"},
                    {60, "a-03"},
                    {61, "a-05"},
                    {62, "a-06"},
                    {63, "a-07"},
                    {64, "a-14"},
                    {65, "a-09"},
                    {66, "a-10"},
                    {67, "a-11"},
                    {68, "a-12"},
                    {69, "a-15"},
                    {70, "b-21"},
                    {71, "b-08"},
                    {72, "b-07"},
                    {73, "b-09"},
                    {74, "b-12"},
                    {75, "b-17"},
                    {76, "b-13"},
                    {77, "b-22"},
                    {78, "d-15"},
                    {79, "d-13"},
                    {80, "d-05"},
                    {81, "d-06"},
                    {82, "d-02"},
                    {83, "d-03"},
                    {84, "d-07"}
                }
            },
            {
                ("Celeste/5-MirrorTemple", AreaMode.BSide),
                new Dictionary<long, string>
                {
                    {0, "start"},
                    {1, "a-00"},
                    {2, "a-01"},
                    {3, "a-02"},
                    {4, "b-00"},
                    {5, "b-06"},
                    {6, "b-01"},
                    {7, "b-02"},
                    {8, "b-05"},
                    {9, "b-08"},
                    {10, "b-09"},
                    {11, "c-00"},
                    {12, "c-01"},
                    {13, "c-02"},
                    {14, "c-03"},
                    {15, "c-04"},
                    {16, "d-00"},
                    {17, "d-01"},
                    {18, "d-02"},
                    {19, "d-03"},
                    {20, "d-04"},
                    {21, "d-05"},
                    {23, "b-07"},
                    {24, "b-03"},
                    {25, "b-04"}
                }
            },
            {
                ("Celeste/5-MirrorTemple", AreaMode.CSide),
                new Dictionary<long, string>
                {
                    {0, "00"},
                    {1, "01"},
                    {2, "02"}
                }
            },
            {
                ("Celeste/6-Reflection", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "start"},
                    {1, "00"},
                    {2, "01"},
                    {3, "02"},
                    {4, "03"},
                    {5, "02b"},
                    {6, "04"},
                    {7, "04e"},
                    {8, "04b"},
                    {9, "05"},
                    {10, "06"},
                    {11, "07"},
                    {12, "08b"},
                    {13, "08a"},
                    {14, "09"},
                    {15, "10b"},
                    {16, "10a"},
                    {17, "11"},
                    {18, "12b"},
                    {19, "12a"},
                    {20, "13"},
                    {21, "14b"},
                    {22, "14a"},
                    {23, "15"},
                    {24, "16b"},
                    {25, "16a"},
                    {26, "17"},
                    {27, "18b"},
                    {28, "18a"},
                    {29, "19"},
                    {30, "20"},
                    {31, "b-00"},
                    {32, "b-01"},
                    {33, "b-02"},
                    {34, "b-02b"},
                    {35, "b-03"},
                    {36, "boss-00"},
                    {37, "boss-01"},
                    {38, "boss-02"},
                    {39, "boss-03"},
                    {40, "boss-04"},
                    {41, "boss-05"},
                    {42, "boss-06"},
                    {43, "boss-07"},
                    {44, "boss-08"},
                    {45, "boss-09"},
                    {46, "boss-10"},
                    {47, "boss-11"},
                    {48, "boss-12"},
                    {49, "boss-13"},
                    {50, "boss-14"},
                    {51, "boss-15"},
                    {52, "boss-16"},
                    {53, "boss-17"},
                    {54, "boss-18"},
                    {55, "boss-19"},
                    {56, "boss-20"},
                    {57, "after-00"},
                    {58, "after-01"},
                    {59, "after-02"},
                    {61, "04c"},
                    {62, "b-00b"},
                    {63, "b-00c"}
                }
            },
            {
                ("Celeste/6-Reflection", AreaMode.BSide),
                new Dictionary<long, string>
                {
                    {0, "a-00"},
                    {1, "a-01"},
                    {2, "a-02"},
                    {3, "a-03"},
                    {4, "a-04"},
                    {5, "a-05"},
                    {6, "a-06"},
                    {7, "b-00"},
                    {8, "b-01"},
                    {9, "b-02"},
                    {10, "b-03"},
                    {11, "b-04"},
                    {12, "b-05"},
                    {13, "b-06"},
                    {14, "b-07"},
                    {15, "b-08"},
                    {16, "b-09"},
                    {17, "b-10"},
                    {18, "c-00"},
                    {19, "c-01"},
                    {20, "c-02"},
                    {21, "c-03"},
                    {22, "c-04"},
                    {23, "d-00"},
                    {24, "d-01"},
                    {25, "d-02"},
                    {26, "d-03"},
                    {27, "d-04"},
                    {28, "d-05"}
                }
            },
            {
                ("Celeste/6-Reflection", AreaMode.CSide),
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
            },
            {
                ("Celeste/7-Summit", AreaMode.BSide),
                new Dictionary<long, string>
                {
                    {0, "a-00"},
                    {1, "a-01"},
                    {2, "a-02"},
                    {3, "a-03"},
                    {4, "b-00"},
                    {5, "b-01"},
                    {6, "b-02"},
                    {7, "b-03"},
                    {8, "c-01"},
                    {9, "c-00"},
                    {10, "c-02"},
                    {11, "c-03"},
                    {12, "d-00"},
                    {13, "d-01"},
                    {14, "d-02"},
                    {15, "d-03"},
                    {16, "e-00"},
                    {17, "e-01"},
                    {18, "e-02"},
                    {19, "e-03"},
                    {20, "f-00"},
                    {21, "f-01"},
                    {22, "f-02"},
                    {23, "f-03"},
                    {24, "g-00"},
                    {25, "g-01"},
                    {26, "g-02"},
                    {27, "g-03"}
                }
            },
            {
                ("Celeste/7-Summit", AreaMode.CSide),
                new Dictionary<long, string>
                {
                    {0, "00"},
                    {1, "01"},
                    {2, "02"}
                }
            },
            {
                ("Celeste/9-Core", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "00"},
                    {1, "0x"},
                    {2, "01"},
                    {3, "02"},
                    {4, "a-00"},
                    {5, "a-01"},
                    {6, "a-02"},
                    {7, "a-03"},
                    {8, "b-00"},
                    {9, "b-06"},
                    {10, "b-07b"},
                    {11, "b-07"},
                    {12, "c-00"},
                    {13, "c-00b"},
                    {14, "c-01"},
                    {15, "c-02"},
                    {16, "c-03"},
                    {17, "c-03b"},
                    {18, "c-04"},
                    {19, "d-00"},
                    {20, "d-01"},
                    {21, "d-02"},
                    {22, "d-03"},
                    {23, "d-04"},
                    {24, "d-05"},
                    {25, "d-06"},
                    {26, "d-07"},
                    {27, "d-08"},
                    {28, "d-09"},
                    {29, "d-10"},
                    {30, "d-10b"},
                    {31, "d-10c"},
                    {32, "d-11"},
                    {33, "space"},
                    {35, "b-02"},
                    {36, "b-01"},
                    {37, "b-03"},
                    {38, "b-04"},
                    {39, "b-05"}
                }
            },
            {
                ("Celeste/9-Core", AreaMode.BSide),
                new Dictionary<long, string>
                {
                    {0, "00"},
                    {1, "01"},
                    {2, "a-00"},
                    {3, "a-01"},
                    {4, "a-02"},
                    {5, "a-03"},
                    {6, "a-04"},
                    {7, "a-05"},
                    {8, "b-00"},
                    {9, "b-01"},
                    {10, "b-02"},
                    {11, "b-03"},
                    {12, "b-04"},
                    {13, "b-05"},
                    {14, "c-00"},
                    {15, "c-01"},
                    {16, "c-02"},
                    {17, "c-03"},
                    {18, "c-04"},
                    {19, "c-05"},
                    {20, "c-06"},
                    {21, "c-08"},
                    {22, "c-07"},
                    {23, "space"}
                }
            },
            {
                ("Celeste/9-Core", AreaMode.CSide),
                new Dictionary<long, string>
                {
                    {0, "intro"},
                    {1, "00"},
                    {2, "01"},
                    {3, "02"}
                }
            },
            {
                ("Celeste/LostLevels", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "intro-01-future"},
                    {1, "intro-02-launch"},
                    {2, "intro-03-space"},
                    {3, "a-00"},
                    {4, "a-01"},
                    {5, "a-02"},
                    {6, "a-03"},
                    {7, "a-04"},
                    {8, "a-05"},
                    {9, "b-00"},
                    {10, "b-01"},
                    {11, "b-02"},
                    {12, "b-03"},
                    {13, "b-04"},
                    {14, "b-05"},
                    {15, "b-06"},
                    {16, "b-07"},
                    {17, "c-00"},
                    {18, "c-00b"},
                    {19, "c-01"},
                    {20, "c-02"},
                    {21, "c-03"},
                    {22, "d-00"},
                    {23, "d-01"},
                    {24, "d-02"},
                    {25, "d-03"},
                    {26, "d-04"},
                    {27, "d-05"},
                    {28, "e-00y"},
                    {29, "e-00z"},
                    {30, "e-00"},
                    {31, "e-00b"},
                    {32, "e-01"},
                    {33, "e-02"},
                    {34, "e-03"},
                    {35, "e-04"},
                    {36, "e-05"},
                    {37, "e-05b"},
                    {38, "e-05c"},
                    {39, "e-06"},
                    {40, "e-07"},
                    {41, "e-08"},
                    {42, "f-door"},
                    {43, "f-00"},
                    {44, "f-01"},
                    {45, "f-02"},
                    {46, "f-03"},
                    {47, "f-04"},
                    {48, "f-05"},
                    {49, "f-06"},
                    {50, "f-07"},
                    {51, "f-08"},
                    {52, "f-09"},
                    {53, "g-00"},
                    {54, "g-01"},
                    {55, "g-03"},
                    {56, "g-02"},
                    {57, "g-04"},
                    {58, "g-05"},
                    {59, "g-06"},
                    {60, "h-00b"},
                    {61, "h-00"},
                    {62, "h-01"},
                    {63, "h-02"},
                    {64, "h-03"},
                    {65, "h-03b"},
                    {66, "h-04"},
                    {67, "h-05"},
                    {68, "h-06"},
                    {69, "h-06b"},
                    {70, "h-07"},
                    {71, "h-08"},
                    {72, "h-09"},
                    {73, "h-10"},
                    {74, "i-00"},
                    {75, "i-00b"},
                    {76, "i-01"},
                    {77, "i-02"},
                    {78, "i-03"},
                    {79, "i-04"},
                    {80, "i-05"},
                    {81, "j-00"},
                    {82, "j-00b"},
                    {83, "j-01"},
                    {84, "j-02"},
                    {85, "j-03"},
                    {86, "j-04"},
                    {87, "j-05"},
                    {88, "j-06"},
                    {89, "j-07"},
                    {90, "j-08"},
                    {91, "j-09"},
                    {92, "j-10"},
                    {93, "j-11"},
                    {94, "j-12"},
                    {95, "j-13"},
                    {96, "j-14"},
                    {97, "j-14b"},
                    {98, "j-15"},
                    {99, "j-16"},
                    {100, "j-17"},
                    {101, "j-18"},
                    {102, "j-19"},
                    {104, "c-alt-00"},
                    {105, "c-alt-01"},
                    {106, "e-00yb"},
                    {107, "h-04b"}
                }
            },
            {
                ("Celeste/0-Intro", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "01"},
                    {1, "-1"},
                    {2, "0b"},
                    {3, "1"},
                    {4, "2"},
                    {5, "3"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/coffe", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "c-01"},
                    {1, "c-02"},
                    {2, "c-03"},
                    {3, "c-04"},
                    {4, "c-05"},
                    {5, "c-06"},
                    {6, "c-07"},
                    {7, "c-08"},
                    {8, "c-08b"},
                    {9, "c-09"},
                    {10, "c-10"},
                    {11, "c-1"},
                    {12, "c-12"},
                    {13, "c-13"},
                    {14, "c-13b"},
                    {15, "c-14"},
                    {16, "c-15"},
                    {17, "c-16"},
                    {18, "c-17"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/asteriskblue", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-01"},
                    {1, "a-02"},
                    {2, "a-03"},
                    {3, "a-04"},
                    {4, "a-05"},
                    {5, "a-06"},
                    {6, "a-07"},
                    {7, "a-08"},
                    {8, "a-09"},
                    {9, "a-10"},
                    {10, "a-11"},
                    {11, "a-12"},
                    {12, "a-13"},
                    {13, "a-16"},
                    {14, "a-14"},
                    {15, "a-17"},
                    {16, "a-15"},
                    {17, "a-18"},
                    {18, "a-19"},
                    {19, "a-20"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/Bing_Over_Google", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "00- intro"},
                    {1, "01- Crusher"},
                    {2, "02- Bait N'Switch"},
                    {3, "02B- a strwawbewwy??"},
                    {4, "03- Uberjump"},
                    {5, "04- Head Trauma"},
                    {6, "05- Boing"},
                    {7, "06- Bubbles"},
                    {8, "07- Falling Cannon"},
                    {9, "07B- OwO whats this??"},
                    {10, "08- U Turn"},
                    {11, "09- Fin"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/cellularAutomaton", AreaMode.Normal),
                new Dictionary<long, string>
                {

                }
            }
        };

        private static Dictionary<(string SID, AreaMode mode), Dictionary<string, long>> roomNameToID =
            roomIdsToname.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Value, y => y.Key));



        public static bool mechanicEnabled(Mechanic mechanic)
        {
            return CelesteArchipelagoModule.SaveData.Mechanics.Contains(getMechanicID(mechanic)) && CelesteArchipelagoModule.IsInArchipelagoSave;
        }
        private static long getMechanicID(Mechanic mechanic)
        {
            return 200000000000 + (int)mechanic;
        }


        //Order matters here, these should be in the order defined in ItemNames.py in the APWorld logic code
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
            WHITE_BLOCK,
            SEEKERS,
            THEO_CRYSTAL,
            KEVIN,
            BUMPER,
            CORE_BLOCK,
            CORE_SWITCH,
            LAVA_ICE_BALLS,
            BREAKER_SWITCH,
            FLYING_BIRD,
            JELLYFISH,
            PUFFER_FISH,
            DOUBLE_DASH_CRYSTAL,
            YELLOW_CASSETTE,
            GREEN_CASSETTE,
            LOOP_BLOCK,
            DREAM_DASH_CRYSTALS,
            INTRO_CRUSHER,
            DASH_ZIP_MOVER
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
