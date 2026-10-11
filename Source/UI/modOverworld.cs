using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.Modifications;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    internal class modOverworld : IGameModification
    {

        delegate IEnumerator orig_Enter(OuiHelper_ChapterSelect_LevelSet self, Oui from);
        Hook enterHook;

        public override void Load()
        {
            enterHook = new Hook(
                typeof(OuiHelper_ChapterSelect_LevelSet).GetMethod("Enter", BindingFlags.Public | BindingFlags.Instance),
                typeof(modOverworld).GetMethod(nameof(OnEnter), BindingFlags.NonPublic | BindingFlags.Static));

            On.Celeste.Overworld.Begin += OnOverworldBegin;
            On.Celeste.OuiChapterSelectIcon.Update += modIconUpdate;
            On.Celeste.OuiChapterSelectIcon.Render += modIconRender;
        }

        public override void Unload()
        {
            enterHook?.Dispose();
            enterHook = null;

            On.Celeste.Overworld.Begin -= OnOverworldBegin;
            On.Celeste.OuiChapterSelectIcon.Update -= modIconUpdate;
            On.Celeste.OuiChapterSelectIcon.Render -= modIconRender;
        }

        static void modIconRender(On.Celeste.OuiChapterSelectIcon.orig_Render orig, OuiChapterSelectIcon self)
        {
            orig(self);

            MTexture mTexture = self.front;
            Vector2 scale = self.Scale;
            int num = mTexture.Width;
            if (self.New && SaveData.Instance != null)
            {
                Vector2 position = self.Position + new Vector2((float)num * 0.25f, (float)(-mTexture.Height) * 0.25f);
                position += Vector2.UnitY * (0f - Math.Abs(self.newWiggle.Value * 30f));
                GFX.Gui["areas/new"].DrawCentered(position);
            }
        }
        
        static void modIconUpdate(On.Celeste.OuiChapterSelectIcon.orig_Update orig, OuiChapterSelectIcon self)
        {
            orig(self);

            if (CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                AreaStats area = SaveData.Instance.Areas_Safe[self.Area];
                string sid = area.SID;
                if (ArchipelagoUtils.isLobbyOrGymSID(sid))
                {
                    LevelCategory category = ArchipelagoMapper.getLevelCategory(sid);
                    HashSet<long> lobbyLevels = ArchipelagoMapper.getLevelsInCategory(category);
                    foreach (long levelID in lobbyLevels)
                    {
                        if (CelesteArchipelagoModule.SaveData.LevelsWithPotentialProgress.Contains(levelID))
                        {
                            self.New = true;
                            return;
                        }
                    }
                }
                else if (ArchipelagoMapper.levelIsAllowed(sid))
                {
                    Dictionary<AreaMode, long?> levelIDs = ArchipelagoUtils.getAllModeLevelIDs(sid);
                    foreach (AreaMode mode in levelIDs.Keys)
                    {
                        if (levelIDs[mode] != null && CelesteArchipelagoModule.SaveData.LevelsWithPotentialProgress.Contains((long)levelIDs[mode]) && !CelesteArchipelagoModule.Settings.HideProgressionHighlighting)
                        {
                            self.New = true;
                            return;
                        }
                    }
                    self.New = false;
                }
                else self.New = false;
            }
        }
        
        static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
        {
            orig(self);
            ArchipelagoUtils.setUnlockedAreas();
        }
        
        static IEnumerator OnEnter(orig_Enter orig, OuiHelper_ChapterSelect_LevelSet self, Oui from)
        {
            if (self.Direction == 0 || !CelesteArchipelagoModule.IsInArchipelagoSave)
                return orig(self, from);
            return CycleAllowed(self);
        }

        static IEnumerator CycleAllowed(OuiHelper_ChapterSelect_LevelSet self)
        {
            int dir = Math.Sign(self.Direction);
            yield return 0.25f;

            int startID = SaveData.Instance.LastArea.ID;
            string startSet = AreaData.Areas[startID].LevelSet;
            int count = AreaData.Areas.Count;

            for (int i = (count + startID + dir) % count; i != startID; i = (count + i + dir) % count)
            {
                AreaData area = AreaData.Areas[i];
                if (area.LevelSet != startSet && ArchipelagoManager.Instance.included_level_sets.Contains(area.LevelSet))
                {
                    SaveData.Instance.LastArea = area.ToKey();
                    break;
                }
            }

            Audio.Play(dir > 0 ? SFX.ui_world_chapter_pane_expand : SFX.ui_world_chapter_pane_contract);
            self.Overworld.Goto<OuiChapterSelect>();
        }
    }
}
