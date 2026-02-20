using Celeste;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.Modifications;
using FMOD;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
            
            DynamicData dynamicOuiChapterSelect = new DynamicData(self);

            Dictionary<int, HashSet<string>> savedCheckpoints = null;
            bool shouldRandomizeCheckpoints = ArchipelagoManager.Instance?.Ready == true
                && ArchipelagoManager.Instance.randomize_checkpoints
                && SaveData.Instance != null
                && CelesteArchipelagoModule.SaveData != null;

            if (shouldRandomizeCheckpoints)
            {
                savedCheckpoints = ApplyCheckpointOverrides(self.Area.SID, self.Area.ID);
            }

            orig(self);

            if (savedCheckpoints != null)
            {
                RestoreCheckpoints(self.Area.ID, savedCheckpoints);
            }

            string sid = self.Area.SID;

            if (sid.StartsWith("Celeste/"))
            {
                var existingModes = new Dictionary<string, Option>();
                foreach (var mode in self.modes)
                {
                    existingModes[mode.ID] = mode;
                }

                self.modes.Clear();

                string bgTexture = (string)dynamicOuiChapterSelect.Invoke("_ModAreaselectTexture", "areaselect/tab");
                string playTexture = (string)dynamicOuiChapterSelect.Invoke("_ModMenuTexture", "menu/play");

                Option aOption = existingModes.TryGetValue("A", out var existingA) ? existingA : new Option();
                aOption.Bg = GFX.Gui[bgTexture];
                aOption.Label = Dialog.Clean(self.Data.Interlude_Safe ? "FILE_BEGIN" : "overworld_normal").ToUpper();
                aOption.Icon = GFX.Gui[playTexture];
                aOption.ID = "A";
                self.modes.Add(aOption);

                if (!self.Data.Interlude_Safe && self.Area.ID < 10)
                {
                    string remixTexture = (string)dynamicOuiChapterSelect.Invoke("_ModMenuTexture", "menu/remix");
                    string rmx2Texture = (string)dynamicOuiChapterSelect.Invoke("_ModMenuTexture", "menu/rmx2");

                    Option bOption = existingModes.TryGetValue("B", out var existingB) ? existingB : new Option();
                    bOption.Label = Dialog.Clean("overworld_remix");
                    bOption.Icon = GFX.Gui[remixTexture];
                    bOption.ID = "B";
                    bOption.Bg = GFX.Gui[bgTexture];
                    self.modes.Add(bOption);

                    Option cOption = existingModes.TryGetValue("C", out var existingC) ? existingC : new Option();
                    cOption.Label = Dialog.Clean("overworld_remix2");
                    cOption.Icon = GFX.Gui[rmx2Texture];
                    cOption.ID = "C";
                    cOption.Bg = GFX.Gui[bgTexture];
                    self.modes.Add(cOption);
                }
            }

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