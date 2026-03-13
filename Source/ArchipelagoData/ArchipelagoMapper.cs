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
            {31, ("StrawberryJam2021/1-Beginner/Ceph", AreaMode.Normal)},
            {32, ("StrawberryJam2021/1-Beginner/Circumplex", AreaMode.Normal)},
            {33, ("StrawberryJam2021/1-Beginner/CoupCritik", AreaMode.Normal)},
            {34, ("StrawberryJam2021/1-Beginner/Eclipse", AreaMode.Normal)},
            {35, ("StrawberryJam2021/1-Beginner/voliver9", AreaMode.Normal)},
            {36, ("StrawberryJam2021/1-Beginner/snas", AreaMode.Normal)},
            {37, ("StrawberryJam2021/1-Beginner/NotYourBadeline", AreaMode.Normal)},
            {38, ("StrawberryJam2021/1-Beginner/Flagpole1up", AreaMode.Normal)},
            {39, ("StrawberryJam2021/1-Beginner/frozenflygone", AreaMode.Normal)},
            {40, ("StrawberryJam2021/1-Beginner/HankyMueller", AreaMode.Normal)},
            {41, ("StrawberryJam2021/1-Beginner/hyperlife", AreaMode.Normal)},
            {42, ("StrawberryJam2021/1-Beginner/Jadeturtle", AreaMode.Normal)},
            {43, ("StrawberryJam2021/1-Beginner/joltik", AreaMode.Normal)},
            {44, ("StrawberryJam2021/1-Beginner/mosscairn", AreaMode.Normal)},
            {45, ("StrawberryJam2021/1-Beginner/Owen-Shirrel", AreaMode.Normal)},
            {46, ("StrawberryJam2021/1-Beginner/Quinnigan", AreaMode.Normal)},
            {47, ("StrawberryJam2021/1-Beginner/skeleton", AreaMode.Normal)},
            {48, ("StrawberryJam2021/1-Beginner/ZZ-HeartSide", AreaMode.Normal)},

            {49, ("StrawberryJam2021/2-Intermediate/Arphimigon", AreaMode.Normal)},
            {50, ("StrawberryJam2021/2-Intermediate/bryse0n", AreaMode.Normal)},
            {51, ("StrawberryJam2021/2-Intermediate/Dooshii", AreaMode.Normal)},
            {52, ("StrawberryJam2021/2-Intermediate/Emik", AreaMode.Normal)},
            {53, ("StrawberryJam2021/2-Intermediate/Evilleafy", AreaMode.Normal)},
            {54, ("StrawberryJam2021/2-Intermediate/Ezel", AreaMode.Normal)},
            {55, ("StrawberryJam2021/2-Intermediate/GlowWoomii", AreaMode.Normal)},
            {56, ("StrawberryJam2021/2-Intermediate/Ice", AreaMode.Normal)},
            {57, ("StrawberryJam2021/2-Intermediate/Jems", AreaMode.Normal)},
            {58, ("StrawberryJam2021/2-Intermediate/LegS", AreaMode.Normal)},
            {59, ("StrawberryJam2021/2-Intermediate/Liero", AreaMode.Normal)},
            {60, ("StrawberryJam2021/2-Intermediate/Luma", AreaMode.Normal)},
            {61, ("StrawberryJam2021/2-Intermediate/marlin", AreaMode.Normal)},
            {62, ("StrawberryJam2021/2-Intermediate/pixelator", AreaMode.Normal)},
            {63, ("StrawberryJam2021/2-Intermediate/Rocketguy2", AreaMode.Normal)},
            {64, ("StrawberryJam2021/2-Intermediate/SpoopySoup", AreaMode.Normal)},
            {65, ("StrawberryJam2021/2-Intermediate/thebreadstick1", AreaMode.Normal)},
            {66, ("StrawberryJam2021/2-Intermediate/vitellary", AreaMode.Normal)},
            {67, ("StrawberryJam2021/2-Intermediate/ZZ-HeartSide", AreaMode.Normal)},

            {68, ("StrawberryJam2021/3-Advanced/astraxel", AreaMode.Normal)},
            {69, ("StrawberryJam2021/3-Advanced/BlueXans", AreaMode.Normal)},
            {70, ("StrawberryJam2021/3-Advanced/Citrea", AreaMode.Normal)},
            {71, ("StrawberryJam2021/3-Advanced/galaksyz", AreaMode.Normal)},
            {72, ("StrawberryJam2021/3-Advanced/Goldian", AreaMode.Normal)},
            {73, ("StrawberryJam2021/3-Advanced/hennyburgr", AreaMode.Normal)},
            {74, ("StrawberryJam2021/3-Advanced/Indecx", AreaMode.Normal)},
            {75, ("StrawberryJam2021/3-Advanced/JANisEXIST", AreaMode.Normal)},
            {76, ("StrawberryJam2021/3-Advanced/jolly", AreaMode.Normal)},
            {77, ("StrawberryJam2021/3-Advanced/Julia", AreaMode.Normal)},
            {78, ("StrawberryJam2021/3-Advanced/Maladroit", AreaMode.Normal)},
            {79, ("StrawberryJam2021/3-Advanced/Meario", AreaMode.Normal)},
            {80, ("StrawberryJam2021/3-Advanced/mmm", AreaMode.Normal)},
            {81, ("StrawberryJam2021/3-Advanced/MousseMoose", AreaMode.Normal)},
            {82, ("StrawberryJam2021/3-Advanced/Nic", AreaMode.Normal)},
            {83, ("StrawberryJam2021/3-Advanced/Oppen", AreaMode.Normal)},
            {84, ("StrawberryJam2021/3-Advanced/pugroy", AreaMode.Normal)},
            {85, ("StrawberryJam2021/3-Advanced/RadleyMcTuneston", AreaMode.Normal)},
            {86, ("StrawberryJam2021/3-Advanced/Roborb", AreaMode.Normal)},
            {87, ("StrawberryJam2021/3-Advanced/sp1029", AreaMode.Normal)},
            {88, ("StrawberryJam2021/3-Advanced/TiltTheStars", AreaMode.Normal)},
            {89, ("StrawberryJam2021/3-Advanced/Tortoise", AreaMode.Normal)},
            {90, ("StrawberryJam2021/3-Advanced/Vamp", AreaMode.Normal)},
            {91, ("StrawberryJam2021/3-Advanced/Viv", AreaMode.Normal)},
            {92, ("StrawberryJam2021/3-Advanced/Worldwaker2", AreaMode.Normal)},
            {93, ("StrawberryJam2021/3-Advanced/ZZ-HeartSide", AreaMode.Normal)},

            {94, ("StrawberryJam2021/4-Expert/Agent", AreaMode.Normal)},
            {95, ("StrawberryJam2021/4-Expert/alicequasar", AreaMode.Normal)},
            {96, ("StrawberryJam2021/4-Expert/Appels", AreaMode.Normal)},
            {97, ("StrawberryJam2021/4-Expert/Archire", AreaMode.Normal)},
            {98, ("StrawberryJam2021/4-Expert/Aspar", AreaMode.Normal)},
            {99, ("StrawberryJam2021/4-Expert/Banana23", AreaMode.Normal)},
            {100, ("StrawberryJam2021/4-Expert/Cabob", AreaMode.Normal)},
            {101, ("StrawberryJam2021/4-Expert/DanTKO", AreaMode.Normal)},
            {102, ("StrawberryJam2021/4-Expert/Flamecrafter113", AreaMode.Normal)},
            {103, ("StrawberryJam2021/4-Expert/fonda1515", AreaMode.Normal)},
            {104, ("StrawberryJam2021/4-Expert/hivemindsrule", AreaMode.Normal)},
            {105, ("StrawberryJam2021/4-Expert/itsabrody", AreaMode.Normal)},
            {106, ("StrawberryJam2021/4-Expert/jackal", AreaMode.Normal)},

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
            {"StrawberryJam2021/1-Beginner/", LevelCategory.BEGINNER},
            {"StrawberryJam2021/2-Intermediate/", LevelCategory.INTERMEDIATE},
            {"StrawberryJam2021/3-Advanced/", LevelCategory.INTERMEDIATE}
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
                    {0, "01"},
                    {1, "02"},
                    {2, "02b"},
                    {3, "03"},
                    {4, "04"},
                    {5, "04b"},
                    {6, "05"},
                    {7, "06"},
                    {8, "07"},
                    {9, "08"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/Ceph", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "1"},
                    {1, "2"},
                    {2, "3"},
                    {3, "4"},
                    {4, "6"},
                    {5, "7"},
                    {6, "8"},
                    {7, "9"},
                    {8, "10"},
                    {9, "11-c"},
                    {10, "12"},
                    {12, "ber4"},
                    {13, "5"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/Circumplex", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "01"},
                    {1, "02"},
                    {2, "03"},
                    {3, "04"},
                    {4, "04b"},
                    {5, "05"},
                    {6, "05b"},
                    {7, "06"},
                    {8, "07"},
                    {9, "07b"},
                    {10, "08"},
                    {11, "09"},
                    {12, "10"},
                    {13, "10b"},
                    {14, "11"},
                    {15, "11b"},
                    {16, "heart"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/CoupCritik", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "01"},
                    {1, "02"},
                    {2, "Berry1"},
                    {3, "03"},
                    {4, "04"},
                    {5, "05"},
                    {6, "06"},
                    {7, "07"},
                    {8, "08"},
                    {9, "09"},
                    {10, "10"},
                    {11, "11"},
                    {12, "12"},
                    {13, "Berry2"},
                    {14, "13"},
                    {15, "14"},
                    {16, "15"},
                    {17, "16"},
                    {18, "17"},
                    {20, "RouteB-1"},
                    {21, "RouteB-2"},
                    {22, "RouteB-3"},
                    {23, "RouteB-4"},
                    {24, "RouteA-2"},
                    {25, "RouteA-1"},
                    {26, "RouteA-3"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/Eclipse", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a_01"},
                    {1, "a_02"},
                    {2, "a_03"},
                    {3, "b_01"},
                    {4, "b_02"},
                    {5, "b_03"},
                    {6, "b_04"},
                    {7, "b_05"},
                    {8, "c_01"},
                    {9, "c_02"},
                    {10, "c_03_end"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/voliver9", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a_01"},
                    {1, "a_02"},
                    {2, "a_03"},
                    {3, "a_04"},
                    {4, "a_05"},
                    {5, "a_06"},
                    {6, "b-01"},
                    {7, "a_07"},
                    {8, "a_08"},
                    {9, "b-02"},
                    {10, "a_09"},
                    {11, "b-03"},
                    {12, "b-04"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/snas", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "1"},
                    {1, "2"},
                    {2, "3"},
                    {3, "4"},
                    {4, "5"},
                    {5, "6"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/NotYourBadeline", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a_01"},
                    {1, "a_02"},
                    {2, "a_03"},
                    {3, "a_04"},
                    {4, "a_05"},
                    {5, "a_10"},
                    {6, "a_06"},
                    {7, "a_07"},
                    {8, "a_08"},
                    {9, "a_09"},
                    {10, "a_11"},
                    {11, "a_12"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/Flagpole1up", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a01"},
                    {1, "a02"},
                    {2, "a02_b"},
                    {3, "a03"},
                    {4, "a03_s"},
                    {5, "a04"},
                    {6, "a05"},
                    {7, "a06"},
                    {8, "a06_s"},
                    {9, "a07"},
                    {10, "a08"},
                    {11, "a09"},
                    {12, "a09_s"},
                    {13, "a09_b"},
                    {14, "a09_b_s"},
                    {15, "a10"},
                    {16, "a11"},
                    {17, "a12"},
                    {18, "a12_s"},
                    {19, "a13"},
                    {20, "a13_b"},
                    {21, "a14_Outro"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/frozenflygone", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "SS2-0"},
                    {1, "SS2-1"},
                    {2, "SS2-2"},
                    {3, "SS2-3"},
                    {4, "SS2-4"},
                    {5, "SS2-5b"},
                    {6, "SS2-6"},
                    {7, "SS2-7"},
                    {8, "HUB"},
                    {9, "FinalChallenge"},
                    {10, "ESCAPE"},
                    {11, "HEART"},
                    {13, "WZ-0"},
                    {14, "WZ-1"},
                    {15, "WZ-2"},
                    {16, "WZ-3a"},
                    {17, "WZ-4"},
                    {18, "WZ-5a"},
                    {19, "WZ-Tele"},
                    {20, "Lab-0"},
                    {21, "Lab-1"},
                    {22, "Lab-2"},
                    {23, "Lab-3"},
                    {24, "Lab-4"},
                    {25, "Lab-5"},
                    {26, "Lab-6"},
                    {27, "Lab-7"},
                    {28, "Lab-Tele"},
                    {29, "Lab-5berry"},
                    {30, "Lab-secret"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/HankyMueller", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "Intro A"},
                    {1, "Intro B"},
                    {2, "Double Vision"},
                    {3, "Waiting Room"},
                    {4, "Timestop Intro"},
                    {5, "Timestop Intro Again"},
                    {6, "Stepping Stone"},
                    {7, "Fork"},
                    {8, "Seeded Berry"},
                    {9, "Easter Egg Puzzle"},
                    {10, "Staircase"},
                    {11, "End"},
                    {12, "End Cabin"},
                    {14, "Shuffle"},
                    {15, "Feedback Loop"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/hyperlife", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-01"},
                    {1, "a-02"},
                    {2, "a-02-b"},
                    {3, "a-04"},
                    {4, "a-05b"},
                    {5, "a-03"},
                    {6, "a-05"},
                    {7, "a-05s"},
                    {8, "a-06"},
                    {9, "a-07"},
                    {10, "a-08"},
                    {11, "a-08s"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/Jadeturtle", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-00"},
                    {1, "a-01z"},
                    {2, "a-02y"},
                    {3, "a-03y"},
                    {4, "a-04z"},
                    {5, "a-05z"},
                    {6, "a-06z"},
                    {7, "a-07z"},
                    {8, "a-08z"},
                    {9, "a-09z"},
                    {10, "a-11z"},
                    {11, "a-10z"},
                    {12, "a-12z"},
                    {13, "a-13z"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/joltik", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a_00"},
                    {1, "a_01"},
                    {2, "a_02"},
                    {3, "a_03"},
                    {4, "a_04"},
                    {5, "a_05"},
                    {6, "a_06"},
                    {7, "a_07"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/mosscairn", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "intro"},
                    {1, "a-00"},
                    {2, "a-01"},
                    {3, "a-02"},
                    {4, "a-03"},
                    {5, "a-03b"},
                    {6, "a-04a"},
                    {7, "a-04b"},
                    {8, "a-05"},
                    {9, "a-06b"},
                    {10, "a-06a"},
                    {11, "a-06c"},
                    {12, "a-07a"},
                    {13, "a-08"},
                    {14, "a-07b"},
                    {15, "gay"},
                    {16, "a-09"},
                    {17, "a-10"},
                    {18, "b-00"},
                    {19, "b-berry00"},
                    {20, "b-01"},
                    {21, "b-02"},
                    {22, "b-03"},
                    {23, "b-berry1"},
                    {24, "b-04"},
                    {25, "b-berry2"},
                    {26, "b-05"},
                    {27, "b-berry3"},
                    {28, "b-06"},
                    {29, "b-06b"},
                    {30, "b-07"},
                    {31, "b-08"},
                    {32, "b-09"},
                    {33, "b-10"},
                    {34, "b-11"},
                    {35, "bus"},
                    {36, "c-intro"},
                    {37, "c-00"},
                    {38, "c-01"},
                    {39, "c-02"},
                    {40, "c-03"},
                    {41, "c-04"},
                    {42, "c-05"},
                    {43, "c-06"},
                    {44, "end"},
                    {46, "b-tribute"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/Owen-Shirrel", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "00 - Overpass"},
                    {1, "01 - Lockdown"},
                    {2, "02 - Breadth"},
                    {3, "03 - Labyrinth"},
                    {4, "03a - Portcullis"},
                    {5, "04 - Widdershins"},
                    {6, "04a - Correlation"},
                    {7, "05 - Symmetry"},
                    {8, "06a - Shackle"},
                    {9, "06 - Socket"},
                    {10, "07 - Ferry"},
                    {11, "08 - Daedalus"},
                    {12, "08a - Reunion"},
                    {13, "09 - Perpendicular"},
                    {14, "10 - Downfall"},
                    {15, "secret"},
                    {17, "04b - Alcove"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/Quinnigan", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "q00"},
                    {1, "q01"},
                    {2, "q02"},
                    {3, "q03"},
                    {4, "q04"},
                    {5, "q05"},
                    {6, "q06"},
                    {7, "q07"},
                    {8, "q08"},
                    {9, "q09"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/skeleton", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "skeleton_00"},
                    {1, "skeleton_01"},
                    {2, "skeleton_02"},
                    {3, "skeleton_02_berry"},
                    {4, "skeleton_03"},
                    {5, "skeleton_04"},
                    {6, "skeleton_05"},
                    {7, "skeleton_outro"}
                }
            },
            {
                ("StrawberryJam2021/1-Beginner/ZZ-HeartSide", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "cp1_heartside_intro"},
                    {1, "cp1_21_heartside_Bing_Over_Google"},
                    {2, "cp1_20_heartside_hyperlife"},
                    {3, "cp1_19_heartside_cellularAutomaton"},
                    {4, "cp1_18_heartside_Eclipse"},
                    {5, "cp2_checkpoint"},
                    {6, "cp2-17-heartside_NotYourBadeline"},
                    {7, "cp2-16-heartside_snas"},
                    {8, "cp2_15_heartside_frozenflygone_a"},
                    {9, "cp2_15_heartside_frozenflygone_b"},
                    {10, "cp2_15_heartside_frozenflygone_c"},
                    {11, "cp2_15_heartside_frozenflygone_d"},
                    {12, "cp3_checkpoint"},
                    {13, "cp3_14_heartside_asterisk"},
                    {14, "cp3_13_heartside_skeleton"},
                    {15, "cp3_12_heartside_coffe"},
                    {16, "cp3_11_heartside_joltik"},
                    {17, "cp4_checkpoint"},
                    {18, "cp4_10_heartside_Hanky"},
                    {19, "cp4_09_heartside_jadeturtle"},
                    {20, "cp4_08_heartside_quinnigan"},
                    {21, "cp5_checkpoint"},
                    {22, "cp5_07_heartside_voliver9"},
                    {23, "cp5_06_heartside_CoupCritik1"},
                    {24, "cp5_06_heartside_CoupCritik2"},
                    {25, "cp5_06_heartside_CoupCritik3"},
                    {26, "cp5_05_Flagpole1up_Heartside"},
                    {27, "cp5_04_heartside_circumplex"},
                    {28, "cp6_checkpoint"},
                    {29, "cp6_03_heartside_awheyaway"},
                    {30, "cp6_02_heartside_Ceph"},
                    {31, "cp6_03_heartside_Moss_1"},
                    {32, "cp6_03_heartside_Moss_2"},
                    {33, "heartside_outro"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/Arphimigon", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a_01"},
                    {1, "a_02"},
                    {2, "a_03"},
                    {3, "a_04"},
                    {4, "a_05"},
                    {5, "b_01"},
                    {6, "b_02"},
                    {7, "b_02b"},
                    {8, "b_03"},
                    {9, "b_04"},
                    {10, "b_05"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/bryse0n", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a_01"},
                    {1, "a-02"},
                    {2, "a_02.5"},
                    {3, "a_03"},
                    {4, "b_01"},
                    {5, "a_04"},
                    {6, "b_02"},
                    {7, "a_05"},
                    {8, "a_06"},
                    {9, "b_03"},
                    {10, "a_07"},
                    {11, "a_08"},
                    {12, "b_04"},
                    {13, "a_09"},
                    {14, "a_10"},
                    {15, "outro"},
                    {16, "hmmmm"},
                    {17, "uwu"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/Dooshii", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a1"},
                    {1, "a1.5v2"},
                    {2, "a2v2"},
                    {3, "a3v2"},
                    {4, "a4v2"},
                    {5, "a5_"},
                    {6, "a6v2"},
                    {7, "r_00v2"},
                    {8, "pushupv2"},
                    {9, "reboundv2"},
                    {10, "a9v2"},
                    {11, "a_10v2"},
                    {12, "r_01v2"},
                    {13, "downmoveblockv2"},
                    {14, "end_but_for_real_this_time"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/Emik", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a_01"},
                    {1, "a_02"},
                    {2, "a_03"},
                    {3, "a_04"},
                    {4, "a_05"},
                    {5, "a_06"},
                    {6, "a_07"},
                    {7, "a_08"},
                    {8, "a_09"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/Evilleafy", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "Evilleafy-00"},
                    {1, "Evilleafy-01"},
                    {2, "Evilleafy-02"},
                    {3, "Evilleafy-03a"},
                    {4, "Evilleafy-04a"},
                    {5, "Evilleafy-06"},
                    {6, "Evilleafy-07"},
                    {7, "Evilleafy-04"},
                    {8, "Evilleafy-08b"},
                    {9, "Evilleafy-09"},
                    {10, "Evilleafy-10"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/Ezel", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "A-01"},
                    {1, "A-02"},
                    {2, "A-03"},
                    {3, "A-04"},
                    {4, "A-05"},
                    {5, "A-05b"},
                    {6, "A-06"},
                    {7, "A-07"},
                    {8, "A-07b"},
                    {9, "A-08"},
                    {10, "A-09"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/GlowWoomii", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "startroom"},
                    {1, "r1"},
                    {2, "r2"},
                    {3, "r3"},
                    {4, "rhub"},
                    {5, "r4"},
                    {6, "r5"},
                    {7, "r6"},
                    {8, "r7"},
                    {9, "r8"},
                    {10, "r9"},
                    {11, "r9sb"},
                    {12, "endroom"},
                    {13, "endroomsecret"},
                    {15, "r6sb"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/Ice", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-01"},
                    {1, "a-02"},
                    {2, "a-03"},
                    {3, "a-04"},
                    {4, "b-01"},
                    {5, "c-01"},
                    {6, "c-02"},
                    {8, "b-05"},
                    {9, "b-06"},
                    {10, "b-07"},
                    {11, "b-02"},
                    {12, "b-03"},
                    {13, "b-04"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/Jems", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "btd-00"},
                    {1, "btd-02"},
                    {2, "btd-02b"},
                    {3, "btd-02c"},
                    {4, "btd-02a"},
                    {5, "btd-03"},
                    {6, "btd-04"},
                    {7, "btd-04a"},
                    {8, "btd-05"},
                    {9, "btd-05a"},
                    {10, "btd-06"},
                    {11, "btd-07"},
                    {12, "btd-09"},
                    {13, "btd-20"},
                    {14, "btd-20a"},
                    {15, "btd-21"},
                    {16, "btd-30"},
                    {17, "btd-31"},
                    {18, "btd-33"},
                    {19, "btd-35"},
                    {20, "btd-42"},
                    {21, "gg"},
                    {23, "btd-12"},
                    {24, "btd-13"},
                    {25, "btd-11"},
                    {26, "btd-10"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/LegS", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "LegS-0"},
                    {1, "LegS-1"},
                    {2, "LegS-2"},
                    {3, "LegS-B1"},
                    {4, "LegS-3"},
                    {5, "LegS-Intermediate"},
                    {6, "LegS-4"},
                    {7, "LegS-B2"},
                    {8, "LegS-5"},
                    {9, "LegS-6"},
                    {10, "LegS-7"},
                    {12, "LegS-CR"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/Liero", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "A00"},
                    {1, "A01"},
                    {2, "A02"},
                    {3, "A03"},
                    {4, "A04"},
                    {5, "A05"},
                    {6, "A06"},
                    {7, "A07"},
                    {8, "A08"},
                    {9, "A09"},
                    {10, "A10"},
                    {11, "A11"},
                    {12, "A12"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/Luma", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "Intro"},
                    {1, "1"},
                    {2, "2"},
                    {3, "2b"},
                    {4, "3"},
                    {5, "4"},
                    {6, "5"},
                    {7, "5b"},
                    {8, "6"},
                    {9, "outro"},
                    {10, "outrob"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/marlin", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "1"},
                    {1, "2"},
                    {2, "3"},
                    {3, "4"},
                    {4, "5"},
                    {5, "5b"},
                    {6, "6"},
                    {7, "7"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/pixelator", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "lvl00"},
                    {1, "lvl01"},
                    {2, "lvl02"},
                    {3, "lvl03"},
                    {4, "lvl04"},
                    {5, "lvl05"},
                    {6, "lvl06"},
                    {7, "lvl07"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/Rocketguy2", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "RG2-0"},
                    {1, "RG2-1"},
                    {2, "RG2-2"},
                    {3, "RG2-3"},
                    {4, "RG2-4"},
                    {5, "RG2-4-S1"},
                    {6, "RG2-4-S2"},
                    {7, "RG2-5"},
                    {8, "RG2-5-S"},
                    {9, "RG2-6"},
                    {10, "RG2-7"},
                    {11, "RG2-8"},
                    {12, "RG2-9"},
                    {13, "RG2-End"},
                    {15, "RG2-huh"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/SpoopySoup", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "soup-1"},
                    {1, "soup-2"},
                    {2, "soup-3"},
                    {3, "soup-4"},
                    {4, "soup-4b"},
                    {5, "soup-5"},
                    {6, "soup-5b"},
                    {7, "soup-6"},
                    {8, "soup-6b"},
                    {9, "soup-7"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/thebreadstick1", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-001"},
                    {1, "a-000"},
                    {2, "a-002"},
                    {3, "a-003"},
                    {4, "a-004"},
                    {5, "a-005"},
                    {6, "a-006"},
                    {7, "a-007"},
                    {8, "a-008"},
                    {9, "a-009"},
                    {10, "a-011"},
                    {11, "a-010"},
                    {12, "a-012"},
                    {13, "a-013"},
                    {15, "a-000S"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/vitellary", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "01"},
                    {1, "02"},
                    {2, "03"},
                    {3, "03-berry"},
                    {4, "04"},
                    {5, "05"},
                    {6, "06"},
                    {7, "06-berry"},
                    {8, "07"},
                    {9, "08"},
                    {10, "09"}
                }
            },
            {
                ("StrawberryJam2021/2-Intermediate/ZZ-HeartSide", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "cp1-0-intro"},
                    {1, "cp1-1-liero"},
                    {2, "cp1-2-pixelator"},
                    {3, "cp1-3-Evilleafy"},
                    {4, "cp1-4-ezel"},
                    {5, "cp2-0-Cp"},
                    {6, "cp2-1-SpoopySoup"},
                    {7, "cp2-2-dooshii"},
                    {8, "cp2-3-glowwoomii"},
                    {9, "cp2-4-ice"},
                    {10, "cp2-5-bryse0n"},
                    {11, "cp3-0-Cp"},
                    {12, "cp3-1-Arphimigon"},
                    {13, "cp3-2-LegS"},
                    {14, "cp3-3-Jems"},
                    {15, "cp3-4-vitellary"},
                    {16, "cp3-5-RG2"},
                    {17, "cp4-0-Cp"},
                    {18, "cp4-1-Emik"},
                    {19, "cp4-2-thebreadstick1"},
                    {20, "cp4-3-Luma"},
                    {21, "cp4-4-Marlin"},
                    {22, "cp4-5-Heart"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/astraxel", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-00"},
                    {1, "a-01"},
                    {2, "a-02"},
                    {3, "a-03"},
                    {4, "a-04-bis"},
                    {5, "a-05"},
                    {6, "a-06"},
                    {7, "a-strawberry"},
                    {8, "b-01"},
                    {9, "b-01-view"},
                    {10, "b-02"},
                    {11, "b-03"},
                    {12, "mini-hearth"},
                    {13, "b-strawberry"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/BlueXans", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "intro"},
                    {1, "1"},
                    {2, "2"},
                    {3, "3"},
                    {4, "berry1"},
                    {5, "reverseTutorial"},
                    {6, "4"},
                    {7, "5"},
                    {8, "outro"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Citrea", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "intro"},
                    {1, "a-1"},
                    {2, "a-2"},
                    {3, "berry0"},
                    {4, "a-3"},
                    {5, "berry1"},
                    {6, "a-4"},
                    {7, "bhop"},
                    {8, "a-5"},
                    {9, "berry2"},
                    {10, "a-6"},
                    {11, "epilogue"},
                    {12, "heart"},
                    {13, "outlook"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/galaksyz", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a_00-Worldwaker2"},
                    {1, "a_01-Gala"},
                    {2, "a_02-Gala"},
                    {3, "a_03-Oppen_heimer"},
                    {4, "a_04-Gala"},
                    {5, "berry-01-Oppen"},
                    {6, "a_05-TiltTheStars"},
                    {7, "a_06-TiltTheStars"},
                    {8, "a_07-TiltTheStars"},
                    {9, "a_08-TiltTheStars"},
                    {10, "heart_room"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Goldian", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "start"},
                    {1, "tutorial-1"},
                    {2, "goldian-1"},
                    {3, "aiden-2"},
                    {4, "goldian-3"},
                    {5, "goldian-4"},
                    {6, "tutorial-2"},
                    {7, "goldian-5"},
                    {8, "goldian-berry"},
                    {9, "goldian-6"},
                    {10, "goldian-7"},
                    {11, "end"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/hennyburgr", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a_01"},
                    {1, "a_02"},
                    {2, "a_03"},
                    {3, "a_04"},
                    {4, "a_05"},
                    {5, "a_06"},
                    {6, "a_06b"},
                    {7, "a_07"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Indecx", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "1"},
                    {1, "2"},
                    {2, "3"},
                    {3, "4"},
                    {4, "5"},
                    {5, "6"},
                    {6, "7"},
                    {7, "8"},
                    {8, "9"},
                    {9, "10"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/JANisEXIST", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "1"},
                    {1, "2"},
                    {2, "3"},
                    {3, "4"},
                    {4, "5"},
                    {5, "6"},
                    {6, "7"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/jolly", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a0"},
                    {1, "a-secret"},
                    {2, "a1"},
                    {3, "a2"},
                    {4, "a-berry"},
                    {5, "a3"},
                    {6, "b1"},
                    {7, "b2"},
                    {8, "b3"},
                    {9, "b4"},
                    {10, "b-berry"},
                    {11, "b5"},
                    {12, "65"},
                    {13, "b7"},
                    {14, "brys1"},
                    {15, "brys2"},
                    {16, "brys3"},
                    {17, "brys4"},
                    {18, "brys-berry"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Julia", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-01"},
                    {1, "a-02"},
                    {2, "a-03"},
                    {3, "a-04"},
                    {4, "a-05intro"},
                    {5, "transition"},
                    {6, "a-05"},
                    {7, "a-07"},
                    {8, "a-06"},
                    {10, "Berry 1"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Maladroit", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "A0"},
                    {1, "A2"},
                    {2, "A2_v2"},
                    {3, "A4"},
                    {4, "A5"},
                    {5, "A5_v2"},
                    {6, "Brys2-2-2"},
                    {7, "A6_v2-flip-2"},
                    {8, "A6"},
                    {9, "A7"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Meario", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-01"},
                    {1, "b-01"},
                    {2, "a-02"},
                    {3, "a-03"},
                    {4, "a-04"},
                    {5, "a-05"},
                    {6, "a-06"},
                    {7, "a-07"},
                    {8, "a-08"},
                    {9, "a-09"},
                    {10, "a-10"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/mmm", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a00"},
                    {1, "a01"},
                    {2, "a02"},
                    {3, "a03"},
                    {4, "a04"},
                    {5, "a05"},
                    {6, "a06"},
                    {7, "a07"},
                    {8, "a07b"},
                    {9, "a08"},
                    {10, "a09"},
                    {11, "a10"},
                    {12, "a10b"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/MousseMoose", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "intro_fall"},
                    {1, "intro_a1"},
                    {2, "a1"},
                    {3, "a2"},
                    {4, "a3"},
                    {5, "a4"},
                    {6, "a5"},
                    {7, "b1"},
                    {8, "b1_b"},
                    {9, "b2"},
                    {10, "b3"},
                    {11, "c1"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Nic", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "01-a"},
                    {1, "02-a"},
                    {2, "03-a"},
                    {3, "05-a"},
                    {4, "06-a"},
                    {5, "06-s1"},
                    {6, "06-b"},
                    {7, "07-a"},
                    {8, "08-a"},
                    {10, "04-a"},
                    {11, "04-s1"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Oppen", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "oppen_intro"},
                    {1, "oppen_1a"},
                    {2, "oppen_berry"},
                    {3, "oppen_1b"},
                    {4, "oppen_1c"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/pugroy", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "BR-00"},
                    {1, "BR-02"},
                    {2, "BR-01"},
                    {3, "BR-08"},
                    {4, "BR-03"},
                    {5, "BR-04"},
                    {6, "BR-07"},
                    {7, "BR-05"},
                    {8, "BR-Outro"},
                    {9, "BR-Extra"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/RadleyMcTuneston", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "start-01-Radley"},
                    {1, "start-02-Radley"},
                    {2, "start-03-Radley/Worldwaker2"},
                    {3, "start-04-Radley"},
                    {4, "start-05-TiltTheStars"},
                    {5, "hub"},
                    {6, "cross-01-Worldwaker2"},
                    {7, "cross-02-TiltTheStars"},
                    {8, "cross-03-TiltTheStars"},
                    {9, "evade-01-Quantum"},
                    {10, "evade-02b-Quantum"},
                    {11, "evade-02-TiltTheStars"},
                    {12, "evade-03-Worldwaker2"},
                    {13, "move-01-TiltTheStars"},
                    {14, "move-02-Worldwaker2"},
                    {15, "move-02b-Quantum"},
                    {16, "escape-01-Worldwaker2"},
                    {17, "escape-02-Worldwaker2"},
                    {18, "escape-03-Worldwaker2"},
                    {19, "start-00-Radley"},
                    {20, "end_HideInMap"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Roborb", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "1-intro"},
                    {1, "1-a"},
                    {2, "1-b"},
                    {3, "1-c"},
                    {4, "1-e"},
                    {5, "1-f"},
                    {6, "1-d"},
                    {7, "1-g"},
                    {8, "1-h"},
                    {9, "2-a"},
                    {10, "2-b"},
                    {11, "2-c"},
                    {12, "2-d"},
                    {13, "2-secret :D"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/sp1029", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-01"},
                    {1, "a-02"},
                    {2, "a-04"},
                    {3, "a-05"},
                    {4, "a-06"},
                    {5, "a-07"},
                    {6, "a-08"},
                    {7, "a-10"},
                    {8, "a-09"},
                    {9, "a-11"},
                    {10, "a-12"},
                    {11, "a-13"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/TiltTheStars", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "intro_v1"},
                    {1, "intro_v2"},
                    {2, "a-01"},
                    {3, "a-02"},
                    {4, "a-03"},
                    {5, "a-04"},
                    {6, "a-05"},
                    {7, "a-06"},
                    {8, "secret-01"},
                    {9, "a-07"},
                    {10, "a-08"},
                    {11, "badeline_v2"},
                    {12, "mini_heart_room"},
                    {14, "secret-02"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Tortoise", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "0b"},
                    {1, "0"},
                    {2, "1"},
                    {3, "1b"},
                    {4, "2"},
                    {5, "2b"},
                    {6, "3"},
                    {7, "3b"},
                    {8, "4"},
                    {9, "4b"},
                    {10, "5"},
                    {11, "6"},
                    {12, "6e"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Vamp", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "intro_SJ"},
                    {1, "Vamp_2"},
                    {2, "Vamp_3"},
                    {3, "Vamp_4"},
                    {4, "Vamp_5"},
                    {5, "Vamp_6"},
                    {6, "Vamp_7"},
                    {7, "Vamp_8"},
                    {8, "Vamp_9"},
                    {9, "Vamp_Final"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Viv", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "viv0"},
                    {1, "viv1"},
                    {2, "viv2"},
                    {3, "viv2b"},
                    {4, "viv3"},
                    {5, "viv3x"},
                    {6, "viv4"},
                    {7, "viv5"},
                    {8, "viv5b"},
                    {9, "viv5x"},
                    {10, "viv6"},
                    {11, "viv7"},
                    {12, "viv7b"},
                    {13, "viv8"},
                    {14, "vivEnd"},
                    {15, "vivEB"},
                    {16, "vivEB_"},
                    {17, "_Endgame"},
                    {18, "vivBonus"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/Worldwaker2", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "1"},
                    {1, "1B"},
                    {2, "2"},
                    {3, "2B"},
                    {4, "3"},
                    {5, "4"},
                    {6, "5"},
                    {7, "6"},
                    {8, "7"},
                    {9, "8"},
                    {10, "9"}
                }
            },
            {
                ("StrawberryJam2021/3-Advanced/ZZ-HeartSide", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "Start"},
                    {1, "heartside_oppen_intro"},
                    {2, "heartside_oppen_a"},
                    {3, "heartside_oppen_b"},
                    {4, "heartside_oppen_c"},
                    {5, "heartside_Worldwaker2"},
                    {6, "heartside_TiltTheStars"},
                    {7, "heartside_Galaksyz"},
                    {8, "heartside_mmm"},
                    {9, "Crest"},
                    {10, "heartside_MousseMoose"},
                    {11, "heartside_Meario"},
                    {12, "heartside_YaGrillRobib"},
                    {13, "heartside_maladroit"},
                    {14, "heartside_pugroy"},
                    {15, "Ravine"},
                    {16, "heartside_astraxel"},
                    {17, "heartside_Tortoise"},
                    {18, "heartside_Tortoise_B"},
                    {19, "heartside_bluexans"},
                    {20, "heartside_Vamp"},
                    {21, "heartside_Julia"},
                    {22, "Aquifer"},
                    {23, "heartside_sp1029"},
                    {24, "heartside_hennyburgr"},
                    {25, "heartside_Indecx"},
                    {26, "heartside_Nic"},
                    {27, "heartside_Ian"},
                    {28, "Landing"},
                    {29, "heartside_citrea"},
                    {30, "heartside_RadleyMcTuneston"},
                    {31, "heartside_Goldian"},
                    {32, "heartside_jolly"},
                    {33, "heartside_Viv"},
                    {34, "Fin"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/Agent", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "Ru_and_AV_and_Zucchini_Are_Cool"},
                    {1, "Agent_00"},
                    {2, "Agent_00a"},
                    {3, "Agent_01"},
                    {4, "Agent_02"},
                    {5, "Agent_03"},
                    {6, "Agent_04"},
                    {7, "Agent_04b"},
                    {8, "Agent_05"},
                    {9, "Agent_06"},
                    {10, "Agent_07"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/alicequasar", AreaMode.Normal),
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
                    {8, "a-08b"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/Appels", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "INTRO1"},
                    {1, "INTRO2"},
                    {2, "a01"},
                    {3, "a01b"},
                    {4, "a02"},
                    {5, "a03"},
                    {6, "a04"},
                    {7, "a05"},
                    {8, "a06new"},
                    {9, "a07"},
                    {10, "a07b"},
                    {11, "a08outro"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/Archire", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-00"},
                    {1, "a-01"},
                    {2, "a-02"},
                    {3, "a-03"},
                    {4, "a-04"},
                    {5, "a-05"},
                    {6, "a-06"},
                    {7, "a-07"},
                    {8, "a-08"},
                    {9, "a-09"},
                    {10, "a-10"},
                    {11, "a-11"},
                    {12, "a-12"},
                    {13, "a-13"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/Aspar", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "00-intro"},
                    {1, "00-intro-cutscene"},
                    {2, "01"},
                    {3, "03"},
                    {4, "05-hub"},
                    {5, "06-crossroad"},
                    {6, "06-berry"},
                    {7, "07"},
                    {8, "07-berry"},
                    {9, "08"},
                    {10, "09"},
                    {11, "11"},
                    {12, "99-end"},
                    {14, "07-berry-2"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/Banana23", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-01"},
                    {1, "a-02"},
                    {2, "a-03"},
                    {3, "a-04"},
                    {4, "a-04b"},
                    {5, "a-05"},
                    {6, "a-06"},
                    {7, "a-06b"},
                    {8, "a-07"},
                    {9, "a-08"},
                    {10, "a-09"},
                    {11, "a-09b"},
                    {12, "a-10"},
                    {13, "a-11"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/Cabob", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-start"},
                    {1, "s-Path of Plane"},
                    {2, "a-00"},
                    {3, "a-01"},
                    {4, "a-02"},
                    {5, "s-Flushed Down"},
                    {6, "a-03"},
                    {7, "a-03x"},
                    {8, "a-04"},
                    {9, "a-05"},
                    {10, "a-06"},
                    {11, "s-Swamp Ascent"},
                    {12, "a-07"},
                    {13, "a-08"},
                    {14, "a-08x"},
                    {15, "a-09"},
                    {16, "a-10"},
                    {17, "a-10x"},
                    {18, "a-end"},
                    {19, "a-end2"},
                    {20, "s-True Ending"},
                    {21, "s-Graveyard"},
                    {23, "s-Water Splash"},
                    {24, "s-Shrek Swamp"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/DanTKO", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "DanTKO_Intro"},
                    {1, "DanTKO_Monolith_1"},
                    {2, "DanTKO_Plane"},
                    {3, "DanTKO_01"},
                    {4, "DanTKO_02"},
                    {5, "DanTKO_blueTutorial_2"},
                    {6, "DanTKO_03"},
                    {7, "DanTKO_04"},
                    {8, "DanTKO_05"},
                    {9, "DanTKO_06"},
                    {10, "DanTKO_06b"},
                    {11, "DanTKO_Berry01"},
                    {12, "DanTKO_07"},
                    {13, "DanTKO_08"},
                    {14, "DanTKO_09"},
                    {15, "DanTKO_Berry02"},
                    {16, "DanTKO_Outro"},
                    {18, "Aperture_Mountain Relic"},
                    {19, "Aperture_Mountain Relic_EXIT"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/Flamecrafter113", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-00-start"},
                    {1, "a-00a"},
                    {2, "a-01"},
                    {3, "a-01_berry"},
                    {4, "a-02"},
                    {5, "a-03"},
                    {6, "a-04"},
                    {7, "a-05"},
                    {8, "a-06"},
                    {9, "a-07"},
                    {10, "a-07_berry"},
                    {11, "a-08"},
                    {12, "a-08_berry"},
                    {13, "a-09"},
                    {14, "a-10-end"},
                    {15, "a-10_berry"},
                    {16, "a-10a"},
                    {18, "a-00c"},
                    {19, "a-00b"},
                    {20, "a-00y"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/fonda1515", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a-00"},
                    {1, "a-01"},
                    {2, "a-02"},
                    {3, "a-03"},
                    {4, "a-04"},
                    {5, "a-05"},
                    {6, "a-05b"},
                    {7, "a-06"},
                    {8, "a-07"},
                    {9, "a-07b"},
                    {10, "secret"},
                    {11, "a-08"},
                    {12, "a-09"},
                    {13, "a-09b"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/hivemindsrule", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a1"},
                    {1, "a2"},
                    {2, "a3"},
                    {3, "a4"},
                    {4, "a5"},
                    {5, "a6"},
                    {6, "a7"},
                    {7, "a8"},
                    {8, "a9"},
                    {9, "b1"},
                    {10, "bones_room"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/itsabrody", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "1"},
                    {1, "2"},
                    {2, "3"},
                    {3, "4"},
                    {4, "5"},
                    {5, "6"},
                    {6, "berry"},
                    {7, "7"},
                    {8, "8"},
                    {9, "9"},
                    {10, "10"}
                }
            },
            {
                ("StrawberryJam2021/4-Expert/jackal", AreaMode.Normal),
                new Dictionary<long, string>
                {
                    {0, "a01"},
                    {1, "a02"},
                    {2, "a04"},
                    {3, "a05"},
                    {4, "a06"},
                    {5, "a07"},
                    {6, "a08"},
                    {7, "a09"},
                    {8, "a10"},
                    {9, "a10b"},
                    {10, "a12"},
                    {11, "a12b"},
                    {12, "a14"},
                    {13, "a15"},
                    {15, "a03"}
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

        public static void logUnlockedMechanics()
        {
            foreach (Mechanic mech in Enum.GetValues(typeof(Mechanic))){
                CelesteArchipelagoModule.Log($"{mech.ToString()} : {mechanicEnabled(mech)}");
            }
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
            DASH_ZIP_MOVER,
            BLUE_CASSETTE_TRAFFIC_BLOCK,
            PINK_CASSETTE_TRAFFIC_BLOCK,
            YELLOW_CASSETTE_TRAFFIC_BLOCK,
            SOAP_BUBBLE,
            DASHLESS_SPRINGS,
            SINGLE_JUMP_REFILL,
            TRIPLE_JUMP_REFILL,
            DOUBLE_DASH_DREAM_BLOCK,
            GREEN_SWITCH_BLOCK,
            ORANGE_SWITCH_BLOCK,
            SWITCH_BLOCK_SWITCH,
            GRAVITY_FIELD,
            BLUE_TIME_CRYSTAL,
            DASH_CRYSTAL_SHARDS,
            PIPE,
            BIG_YELLOW_BUTTON,
            PUZZLE_KEVIN,
            TRIPLE_BOOST_FLOWER,
            BOUNCE_DREAM_BLOCKS,
            ORANGE_LINKED_TRAFFIC_BLOCK,
            BLUE_LINKED_TRAFFIC_BLOCK,
            TOGGLE_SWAP_BLOCK,
            GREEN_LINKED_TRAFFIC_BLOCK,
            STRAWBERRY_JAM,
            NEON_BLUE_KEVIN,
            NEON_PURPLE_KEVIN,
            PUSH_BLOCK,
            VERTIGO_LINKED_TELEPORT,
            YELLOW_PORTAL,
            PURPLE_PORTAL,
            PUSH_STATION_BLOCK,
            BLUE_PORTAL,
            RED_PORTAL,
            TIMED_TOUCH_SWITCH,
            HONEY_BUBBLES,
            ZIPLINE,
            YELLOW_LINKED_TRAFFIC_BLOCK,
            YELLOW_LASER,
            TORQUOISE_LASER,
            GREEN_LASER,
            MAGENTA_LINKED_TRAFFIC_BLOCK,
            TORQUOISE_LINKED_TRAFFIC_BLOCK,
            RED_LINKED_TRAFFIC_BLOCK,
            CRYSTAL_BOMB,
            BOWL_PUFFER,
            PURPLE_DASHLESS_BUBBLE,
            PULL_STATION_BLOCK,
            DASH_SPRING,
            TRACK_SWITCH_BOX,
            PURPLE_LINKED_TRAFFIC_BLOCK,
            GRAY_TIME_CRYSTAL,
            PURPLE_JELLYFISH,
            DASH_REFILL_WALL,
            DOUBLE_DASH_REFILL_WALL,
            GRAY_BUBBLES,
            SWITCH_CRATE,
            GREEN_PORTAL,
            MOVING_TOUCH_SWITCH,
            DREAM_TRAFFIC_BLOCK,
            BOUNCY_SPIKES,
            MOMENTUM_SPRING,
            PURPLE_CASSETTE_BLOCK,
            ORANGE_CASSETTE_BLOCK,
            RED_CASSETTE_BLOCK,
            GREEN_CASSETTE_TRAFFIC_BLOCK,
            PURPLE_MOVING_CASSETTE_BLOCK,
            BLUE_MOVING_CASSETTE_BLOCK,
            PURPLE_CASSETTE_SWAP_BLOCK,
            PINK_CASSETTE_SWAP_BLOCK,
            RED_CASSETTE_SWAP_BLOCK,
            ORANGE_MOVING_CASSETTE_BLOCK,
            ORANGE_CASSETTE_TRAFFIC_BLOCK,
            GREEN_CASSETTE_SWAP_BLOCK,
            RED_CASSETTE_TRAFFIC_BLOCK,
            YELLOW_CASSETTE_SWAP_BLOCK,
            PINK_MOVING_CASSETTE_BLOCK,
            GREEN_MOVING_CASSETTE_BLOCK,
            WORMHOLE_BUBBLE,
            FAKE_CRYSTAL_HEART,
            MINI_FAKE_CRYSTAL_HEART,
            THROW_BOX,
            PURPLE_REBOUND_BOOSTER,
            GROWTH_POTION,
            BLUE_GRAVITY_SPRING,
            RED_GRAVITY_SPRING,
            PURPLE_GRAVITY_SPRING,
            YELLOW_MOVING_CASSETTE_BLOCK,
            MOVE_BLOCK_ACCELERATOR_FIELD,
            MOVE_BLOCK_DELETE_FIELD,
            MOVE_BLOCK_DECELERATOR_FIELD,
            MOVE_BLOCK_REDIRECT_FIELD,
            DREAM_MOVE_BLOCK,
            BATTERY,
            RED_PROPELLER_BLOCK,
            YELLOW_PROPELLER_BLOCK,
            DASH_BOOST_FIELD,
            FORCE_JUMP_CRYSTAL,
            NO_STAMINA_DASH_CRYSTAL,
            SPEED_MUSHROOMS,
            SPEED_MUSHROOM_WALL,
            WHITE_DREAM_BLOCK,
            RED_SPEED_MOSS,
            BLUE_BOUNCE_MOSS,
            TAN_LINKED_TRAFFIC_BLOCK,
            SQUARE_BUMPER,
            BLUE_FLOATING_FIELDS,
            RED_FLOATING_FIELDS,
            PURPLE_FLOATING_FIELDS,
            BLUE_FLIP_SWITCH,
            GREEN_FLIP_SWITCH,
            PINK_FLIP_SWITCH,
            PURPLE_FLIP_SWITCH,
            PINK_SWITCH_BLOCK,
            PURPLE_SWITCH_BLOCK,
            CLOUD_CRYSTAL,
            JELLYFISH_CRYSTAL,
            REWIND_CRYSTAL,
            CURVED_TRAFFIC_BLOCK,
            REVERSE_JELLY,
            INFINITE_DASH_CRYSTAL
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
