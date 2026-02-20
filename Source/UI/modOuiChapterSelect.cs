using Celeste;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.Modifications;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.OuiChapterPanel;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    public class modOuiChapterSelect : IGameModification
    {
        private static Hook _getCheckpointsHook;

        public override void Load()
        {
            On.Celeste.OuiChapterPanel.Reset += modReset;
            On.Celeste.OuiChapterPanel.Start += modStartLevelGuard;

            var getCheckpoints = typeof(SaveData).GetMethod(
                "_GetCheckpoints",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (getCheckpoints != null)
            {
                _getCheckpointsHook = new Hook(
                    getCheckpoints,
                    typeof(modOuiChapterSelect).GetMethod(
                        nameof(GetCheckpoints),
                        BindingFlags.NonPublic | BindingFlags.Static)
                );
            }
        }


        public override void Unload()
        {
            On.Celeste.OuiChapterPanel.Reset -= modReset;
            On.Celeste.OuiChapterPanel.Start -= modStartLevelGuard;

            _getCheckpointsHook?.Dispose();
            _getCheckpointsHook = null;
        }

        private delegate HashSet<string> orig_GetCheckpoints(SaveData save, AreaKey area);
        private static HashSet<string> GetCheckpoints(orig_GetCheckpoints orig, SaveData save, AreaKey area)
        {
            HashSet<string> result = orig(save, area);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave
                || ArchipelagoManager.Instance?.Ready != true
                || !ArchipelagoManager.Instance.randomize_checkpoints
                || CelesteArchipelagoModule.SaveData == null)
            {
                return result;
            }

            string sid = area.SID;
            AreaMode mode = area.Mode;
            AreaData areaData = AreaData.Get(sid);
            if (areaData?.Mode[(int)mode]?.Checkpoints == null)
            {
                return result;
            }

            var filtered = new HashSet<string>();
            foreach (string entry in result)
            {
                string roomName = entry.Contains("|") ? entry.Substring(entry.IndexOf('|') + 1) : entry;
                if (CelesteArchipelagoModule.SaveData.UnlockedCheckpoints.Contains(
                        ArchipelagoMapper.getCheckpointItemID(sid, mode, roomName)))
                {
                    filtered.Add(entry);
                }
            }

            return filtered;
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

            orig(self);

            string sid = self.Area.SID;

            if (sid.StartsWith("Celeste/"))
            {
                self.modes.Clear();
                self.modes.Add(new Option
                {
                    Bg = GFX.Gui[(string)dynamicOuiChapterSelect.Invoke("_ModAreaselectTexture", "areaselect/tab")],
                    Label = Dialog.Clean(self.Data.Interlude_Safe ? "FILE_BEGIN" : "overworld_normal").ToUpper(),
                    Icon = GFX.Gui[(string)dynamicOuiChapterSelect.Invoke("_ModMenuTexture", "menu/play")],
                    ID = "A"
                });
                if (!self.Data.Interlude_Safe && self.Area.ID < 10)
                {
                    self.modes.Add(new Option
                    {
                        Label = Dialog.Clean("overworld_remix"),
                        Icon = GFX.Gui[(string)dynamicOuiChapterSelect.Invoke("_ModMenuTexture", "menu/remix")],
                        ID = "B",
                        Bg = GFX.Gui[(string)dynamicOuiChapterSelect.Invoke("_ModAreaselectTexture", "areaselect/tab")]
                    });
                    self.modes.Add(new Option
                    {
                        Label = Dialog.Clean("overworld_remix2"),
                        Icon = GFX.Gui[(string)dynamicOuiChapterSelect.Invoke("_ModMenuTexture", "menu/rmx2")],
                        ID = "C",
                        Bg = GFX.Gui[(string)dynamicOuiChapterSelect.Invoke("_ModAreaselectTexture", "areaselect/tab")]
                    });
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