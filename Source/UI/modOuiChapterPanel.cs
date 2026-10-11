using Celeste;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.Modifications;
using FMOD;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Xna.Framework;
using static Celeste.OuiChapterPanel;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    public class modOuiChapterPanel : IGameModification
    {

        public override void Load()
        {
            On.Celeste.OuiChapterPanel.Reset += modReset;
            On.Celeste.OuiChapterPanel.Start += modStartLevelGuard;
            On.Celeste.OuiChapterPanel.Update += modUpdate;
            On.Celeste.OuiChapterPanel.IncrementStats += modIncrementStats;
            On.Celeste.OuiChapterPanel.Render += modRender;
            On.Celeste.OuiChapterPanel.Swap += modSwap;
        }

        public override void Unload()
        {
            On.Celeste.OuiChapterPanel.Reset -= modReset;
            On.Celeste.OuiChapterPanel.Start -= modStartLevelGuard;
            On.Celeste.OuiChapterPanel.Update -= modUpdate;
            On.Celeste.OuiChapterPanel.IncrementStats -= modIncrementStats;
            On.Celeste.OuiChapterPanel.Render -= modRender;
            On.Celeste.OuiChapterPanel.Swap -= modSwap;
        }

        private void modSwap(On.Celeste.OuiChapterPanel.orig_Swap orig, OuiChapterPanel self)
        {
            string sid = self.Area.SID;
            AreaMode mode = self.options[self.option].ID == "A" ? AreaMode.Normal : self.options[self.option].ID == "B" ? AreaMode.BSide : self.options[self.option].ID == "C" ? AreaMode.CSide : AreaMode.Normal;
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || ArchipelagoUtils.levelIsUnlocked(sid, mode)) orig(self);
            else Audio.Play("event:/ui/main/button_back");
        }

        private void modRender(On.Celeste.OuiChapterPanel.orig_Render orig, OuiChapterPanel self)
        {   
            orig(self);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave) return;

            string sid = self.Area.SID;

            foreach(Option option in self.modes)
            {
                AreaMode mode = option.ID == "A" ? AreaMode.Normal : option.ID == "B" ? AreaMode.BSide : option.ID == "C" ? AreaMode.CSide : AreaMode.Normal;

                if (ArchipelagoUtils.levelIsUnlocked(sid, mode) && CelesteArchipelagoModule.SaveData.LevelsWithPotentialProgress.Contains((long)ArchipelagoMapper.getLevelID(sid, mode)) && !CelesteArchipelagoModule.Settings.HideProgressionHighlighting)
                {
                    option.BgColor = Color.Lime;
                }
                else if(ArchipelagoUtils.levelIsUnlocked(sid, mode))
                {
                    option.BgColor = Color.Blue;
                }
                else if(ArchipelagoUtils.levelIncludedInRandomizer(sid, mode))
                {
                    option.BgColor = new Color(50, 50, 50);
                }
                else
                {
                    option.BgColor = Color.DarkRed;
                }
            }            
        }

        private void modUpdate(On.Celeste.OuiChapterPanel.orig_Update orig, OuiChapterPanel self)
        {
            orig(self);
            if (CelesteArchipelagoModule.IsInArchipelagoSave && self.Area.SID.StartsWith("StrawberryJam2021") && !ArchipelagoUtils.canEnter(self.Area.SID, AreaMode.Normal))
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

            if (ArchipelagoUtils.canEnter(sid, mode) && canUseCheckpoint(sid, mode, checkpoint))
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

            Dictionary<int, HashSet<string>> savedCheckpoints = null;
            bool allowed = ArchipelagoMapper.levelIsAllowed(sid);

            if (ArchipelagoManager.Instance.randomize_checkpoints && allowed)
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
            self.option = -1;
            for (int i = 0; i < self.modes.Count; i++)
            {
                Option mode = self.modes[i];
                AreaMode areaMode = mode.ID == "C" ? AreaMode.CSide : mode.ID == "B" ? AreaMode.BSide : AreaMode.Normal;
                if (!allowed || !ArchipelagoUtils.levelIncludedInRandomizer(sid, areaMode))
                {
                    mode.Label = "NOT IN ARCHIPELAGO";
                }
                else if (!ArchipelagoUtils.levelIsUnlocked(sid, areaMode))
                {
                    mode.Label = "LOCKED";
                }
                else if (ArchipelagoUtils.isGoalLevel(sid, areaMode) && ArchipelagoUtils.berriesMissing())
                {
                    mode.Label = CelesteArchipelagoModule.SaveData.Strawberries < ArchipelagoManager.Instance.required_strawberries ?
                        $"STRAWBERRIES: {CelesteArchipelagoModule.SaveData.Strawberries} / {ArchipelagoManager.Instance.required_strawberries}" :
                        ArchipelagoManager.Instance.require_moon_berry && !CelesteArchipelagoModule.SaveData.moonBerryCollected ? "MISSING MOON BERRY" : "CLIMB";
                }
                if(ArchipelagoUtils.levelIsUnlocked(sid, areaMode) && self.option == -1)
                {
                    self.option = i;
                }
            }
            if(self.option == -1)
            {
                self.option = 0;
            }
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

            if (sid.StartsWith("StrawberryJam2021")) return ArchipelagoUtils.canEnter(sid, mode);
            else return CelesteArchipelagoModule.SaveData.UnlockedCheckpoints.Contains(ArchipelagoMapper.getCheckpointItemID(sid, mode, checkpoint));
        }
    }
}