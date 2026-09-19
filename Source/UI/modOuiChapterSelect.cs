using Celeste;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.Modifications;
using FMOD;
using MonoMod.Utils;
using System;
using System.Collections;
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
            On.Celeste.OuiChapterPanel.Update += modUpdate;
            On.Celeste.OuiChapterPanel.IncrementStats += modIncrementStats;
        }

        public override void Unload()
        {
            On.Celeste.OuiChapterPanel.Reset -= modReset;
            On.Celeste.OuiChapterPanel.Start -= modStartLevelGuard;
            On.Celeste.OuiChapterPanel.Update -= modUpdate;
            On.Celeste.OuiChapterPanel.IncrementStats -= modIncrementStats;
        }

        private void modUpdate(On.Celeste.OuiChapterPanel.orig_Update orig, OuiChapterPanel self)
        {
            orig(self);
            if (CelesteArchipelagoModule.IsInArchipelagoSave && self.Area.SID.StartsWith("StrawberryJam2021") && !canEnter(self.Area.SID, AreaMode.Normal))
            {
                foreach (Option checkpoint in self.checkpoints)
                {
                    checkpoint.Label = "LOCKED";
                }
            }
        }

        private IEnumerator modIncrementStats(On.Celeste.OuiChapterPanel.orig_IncrementStats orig, OuiChapterPanel self, bool shouldAdvance)
        {
            self.DisplayedStats.Cassette = true;
            yield return orig(self, shouldAdvance);
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
            orig(self);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                return;
            }

            DynamicData dynamicOuiChapterSelect = new DynamicData(self);
            string sid = self.Area.SID;
            CelesteArchipelagoModule.Log(sid);

            Dictionary<int, HashSet<string>> savedCheckpoints = null;
            bool mapped = ArchipelagoMapper.levelSIDToID.ContainsKey((sid, AreaMode.Normal)) || ArchipelagoManager.PermanentUnlockLevels.Contains(sid);

            if (ArchipelagoManager.Instance.randomize_checkpoints && mapped)
            {
                savedCheckpoints = GetUnlockedCheckpointsByMode(sid);
            }

            if (sid.StartsWith("Celeste/") && !self.Data.Interlude_Safe && self.Area.ID < 10)
            {
                if (!self.modes.Any(m => m.ID == "B"))
                {
                    self.modes.Add(new Option
                    {
                        Label = Dialog.Clean("overworld_remix"),
                        Icon = GFX.Gui[(string)dynamicOuiChapterSelect.Invoke("_ModMenuTexture", "menu/remix")],
                        ID = "B",
                        Bg = GFX.Gui[(string)dynamicOuiChapterSelect.Invoke("_ModAreaselectTexture", "areaselect/tab")]
                    });
                }
                if (!self.modes.Any(m => m.ID == "C"))
                {
                    self.modes.Add(new Option
                    {
                        Label = Dialog.Clean("overworld_remix2"),
                        Icon = GFX.Gui[(string)dynamicOuiChapterSelect.Invoke("_ModMenuTexture", "menu/rmx2")],
                        ID = "C",
                        Bg = GFX.Gui[(string)dynamicOuiChapterSelect.Invoke("_ModAreaselectTexture", "areaselect/tab")]
                    });
                }
            }

            if (savedCheckpoints != null)
            {
                foreach (var kvp in savedCheckpoints)
                {
                    SaveData.Instance.Areas_Safe[self.Area.ID].Modes[kvp.Key].Checkpoints = kvp.Value;
                }
            }

            foreach (Option mode in self.modes)
            {
                AreaMode areaMode = mode.ID == "C" ? AreaMode.CSide : mode.ID == "B" ? AreaMode.BSide : AreaMode.Normal;
                if (!mapped)
                {
                    mode.Label = "NOT IN ARCHIPELAGO";
                }
                else if (!levelUnlocked(sid, areaMode))
                {
                    mode.Label = "LOCKED";
                }
                else if (isGoalLevel(sid, areaMode) && berriesMissing())
                {
                    mode.Label = CelesteArchipelagoModule.SaveData.Strawberries < ArchipelagoManager.Instance.required_strawberries ?
                        $"STRAWBERRIES: {CelesteArchipelagoModule.SaveData.Strawberries} / {ArchipelagoManager.Instance.required_strawberries}" :
                        ArchipelagoManager.Instance.require_moon_berry && !CelesteArchipelagoModule.SaveData.moonBerryCollected ? "MISSING MOON BERRY" : "CLIMB";
                }
            }
        }

        private static bool levelUnlocked(string sid, AreaMode areaMode)
        {
            return (ArchipelagoManager.PermanentUnlockLevels.Contains(sid) && areaMode == AreaMode.Normal) ||
                (ArchipelagoMapper.levelSIDToID.ContainsKey((sid, AreaMode.Normal)) &&
                (CelesteArchipelagoModule.SaveData.LevelUnlocks.Contains((sid, areaMode)) ||
                ArchipelagoMapper.getLevelCategory(sid, areaMode) == ArchipelagoManager.Instance.starting_category));
        }

        private static bool berriesMissing()
        {
            return CelesteArchipelagoModule.SaveData.Strawberries < ArchipelagoManager.Instance.required_strawberries || (ArchipelagoManager.Instance.require_moon_berry && !CelesteArchipelagoModule.SaveData.moonBerryCollected);
        }

        private static bool isGoalLevel(string sid, AreaMode areaMode)
        {
            return ArchipelagoManager.Instance.win_condition_level == (sid, areaMode);
        }

        private static bool canEnter(string sid, AreaMode areaMode)
        {
            return levelUnlocked(sid, areaMode) && !isGoalLevel(sid, areaMode) || levelUnlocked(sid, areaMode) && !berriesMissing();
        }

        private static Dictionary<int, HashSet<string>> GetUnlockedCheckpointsByMode(string sid)
        {
            AreaData areaData = AreaData.Get(sid);
            if (areaData == null) return null;

            var saved = new Dictionary<int, HashSet<string>>();

            for (int m = 0; m < areaData.Mode.Length; m++)
            {
                ModeProperties modeData = areaData.Mode[m];
                if (modeData == null) continue;

                HashSet<string> unlockedModeCheckpoints = new HashSet<string>();

                if (modeData.Checkpoints != null)
                {
                    AreaMode mode = (AreaMode)m;
                    for (int i = 0; i < modeData.Checkpoints.Length; i++)
                    {
                        string room = modeData.Checkpoints[i].Level;
                        if (CelesteArchipelagoModule.SaveData.UnlockedCheckpoints.Contains(ArchipelagoMapper.getCheckpointItemID(sid, mode, room)))
                        {
                            unlockedModeCheckpoints.Add(room);
                        }
                    }
                }

                saved[m] = unlockedModeCheckpoints;
            }

            return saved;
        }

        private static bool canUseCheckpoint(string sid, AreaMode mode, string checkpoint)
        {
            if (checkpoint == null)
                return true;
            if (ArchipelagoManager.Instance?.Ready != true || !ArchipelagoManager.Instance.randomize_checkpoints)
                return true;
            if (CelesteArchipelagoModule.SaveData == null)
                return true;

            if (sid.StartsWith("StrawberryJam2021")) return canEnter(sid, mode);
            else return CelesteArchipelagoModule.SaveData.UnlockedCheckpoints.Contains(ArchipelagoMapper.getCheckpointItemID(sid, mode, checkpoint));
        }
    }
}