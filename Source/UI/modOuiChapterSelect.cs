using Celeste;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.Modifications;
using FMOD;
using System;
using System.Collections.Generic;
using static Celeste.OuiChapterPanel;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    public class modOuiChapterSelect : IGameModification
    {

        public override void Load()
        {
            On.Celeste.OuiChapterPanel.Reset += modReset;
            On.Celeste.OuiChapterPanel.Start += modStartLevelGuard;
        }


        public override void Unload()
        {
            On.Celeste.OuiChapterPanel.Reset -= modReset;
            On.Celeste.OuiChapterPanel.Start -= modStartLevelGuard;
        }

        static bool levelGuardRunning = false;
        private static void modStartLevelGuard(On.Celeste.OuiChapterPanel.orig_Start orig, OuiChapterPanel self, string checkpoint)
        {
            if (levelGuardRunning || !CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self, checkpoint);
                return;
            }
            levelGuardRunning = true;
            string sid = self.Area.SID;
            AreaMode mode = self.Area.Mode;

            if (canEnter(sid, mode) && canUseCheckpoint(sid, mode, checkpoint))
            {
                orig(self, checkpoint);
            }
            else
            {
                Audio.Play("event:/ui/main/button_back");
            }
            levelGuardRunning = false;
        }

        private static void modReset(On.Celeste.OuiChapterPanel.orig_Reset orig, OuiChapterPanel self)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self);
                return;
            }
            
            Dictionary<int, HashSet<string>> savedCheckpoints = null;
            bool shouldRandomizeCheckpoints = ArchipelagoManager.Instance?.Ready == true
                && ArchipelagoManager.Instance.randomize_checkpoints
                && SaveData.Instance != null
                && CelesteArchipelagoModule.SaveData != null;

            if (shouldRandomizeCheckpoints)
            {
                savedCheckpoints = ApplyCheckpointOverrides(self.Area.SID, self.Area.ID);
            }

            bool ensureBCModes = SaveData.Instance != null
                && self.Area.SID?.StartsWith("Celeste/") == true
                && self.Data?.Interlude_Safe == false
                && self.Area.ID < 10;
            bool savedCassette = false;
            bool savedBSideHeart = false;

            if (ensureBCModes)
            {
                var areaStats = SaveData.Instance.Areas_Safe[self.Area.ID];
                savedCassette = areaStats.Cassette;
                areaStats.Cassette = true;
                savedBSideHeart = areaStats.Modes[(int)AreaMode.BSide].HeartGem;
                areaStats.Modes[(int)AreaMode.BSide].HeartGem = true;
            }

            orig(self);

            if (ensureBCModes)
            {
                var areaStats = SaveData.Instance.Areas_Safe[self.Area.ID];
                areaStats.Cassette = savedCassette;
                areaStats.Modes[(int)AreaMode.BSide].HeartGem = savedBSideHeart;
            }

            if (savedCheckpoints != null)
            {
                RestoreCheckpoints(self.Area.ID, savedCheckpoints);
            }

            string sid = self.Area.SID;

            foreach (Option mode in self.modes)
            {
                AreaMode areaMode = mode.ID == "C" ? AreaMode.CSide : mode.ID == "B" ? AreaMode.BSide : AreaMode.Normal;
                if (!canEnter(sid, areaMode))
                {
                    mode.Label = "LOCKED";
                }
            }
        }

        private static bool canEnter(string sid, AreaMode areaMode)
        {
            return CelesteArchipelagoModule.SaveData.LevelUnlocks.Contains((sid, areaMode)) || ArchipelagoManager.PermanentUnlockLevels.Contains(sid);
        }

        private static Dictionary<int, HashSet<string>> ApplyCheckpointOverrides(string sid, int areaID)
        {
            AreaData areaData = AreaData.Get(sid);
            if (areaData == null) return null;

            var saved = new Dictionary<int, HashSet<string>>();

            for (int m = 0; m < areaData.Mode.Length; m++)
            {
                var modeData = areaData.Mode[m];
                if (modeData == null) continue;

                AreaModeStats stats = SaveData.Instance.Areas_Safe[areaData.ID].Modes[m];
                saved[m] = stats.Checkpoints;
                stats.Checkpoints = new HashSet<string>();

                if (modeData.Checkpoints != null)
                {
                    AreaMode mode = (AreaMode)m;
                    for (int i = 0; i < modeData.Checkpoints.Length; i++)
                    {
                        string room = modeData.Checkpoints[i].Level;
                        if (CelesteArchipelagoModule.SaveData.UnlockedCheckpoints.Contains(ArchipelagoMapper.getCheckpointItemID(sid, mode, room)))
                        {
                            stats.Checkpoints.Add(room);
                        }
                    }
                }
            }

            return saved;
        }


        private static void RestoreCheckpoints(int areaID, Dictionary<int, HashSet<string>> savedCheckpoints)
        {
            foreach (var kvp in savedCheckpoints)
            {
                SaveData.Instance.Areas_Safe[areaID].Modes[kvp.Key].Checkpoints = kvp.Value;
            }
        }

        private static bool canUseCheckpoint(string sid, AreaMode mode, string checkpoint)
        {
            if (checkpoint == null)
                return true;
            if (ArchipelagoManager.Instance?.Ready != true || !ArchipelagoManager.Instance.randomize_checkpoints)
                return true;
            if (CelesteArchipelagoModule.SaveData == null)
                return true;

            return CelesteArchipelagoModule.SaveData.UnlockedCheckpoints.Contains(ArchipelagoMapper.getCheckpointItemID(sid, mode, checkpoint));
        }
    }
}