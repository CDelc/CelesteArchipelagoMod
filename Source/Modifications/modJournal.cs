using Celeste.Mod.CelesteArchipelago.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using Celeste.Mod.CollabUtils2.UI;
using System.Reflection;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modJournal : IGameModification
    {

        private static Dictionary<string, string> cleanNameToSidLookup;

        private static Hook hookGeneratePages;
        private static Type CollabJournalType;
        private static FieldInfo TableField;
        private delegate List<OuiJournalCollabProgressInLobby> orig_GeneratePages(OuiJournal journal, string levelSet, bool showOnlyDiscovered);

        public override void Load()
        {
            On.Celeste.OuiJournalProgress.ctor += modCtor;
            On.Celeste.OuiJournal.Enter += modEnter;

            CollabJournalType = typeof(OuiJournalCollabProgressInLobby);
            MethodInfo GeneratePagesMethod = CollabJournalType.GetMethod("GeneratePages", BindingFlags.Public | BindingFlags.Static);
            TableField = CollabJournalType.GetField("table", BindingFlags.NonPublic | BindingFlags.Instance);
            hookGeneratePages = new Hook(GeneratePagesMethod, typeof(modJournal).GetMethod(nameof(modGeneratePages), BindingFlags.NonPublic | BindingFlags.Static));

        }

        public override void Unload()
        {
            On.Celeste.OuiJournalProgress.ctor -= modCtor;
            On.Celeste.OuiJournal.Enter -= modEnter;
            hookGeneratePages?.Dispose();
        }

        private static IEnumerator modEnter(On.Celeste.OuiJournal.orig_Enter orig, OuiJournal self, Oui from)
        {
            IEnumerator origReturn = orig(self, from);

            while (origReturn.MoveNext())
            {
                yield return origReturn.Current;
            }

            List<OuiJournalPage> toRemove = new List<OuiJournalPage>();
            
            foreach (OuiJournalPage page in self.Pages)
            {
                if(page is OuiJournalCover || page is OuiJournalProgress || page is OuiJournalCollabProgressInLobby)
                {
                    continue;
                }
                else
                {
                    toRemove.Add(page);
                }
            }

            foreach (OuiJournalPage page in toRemove)
            {
                self.Pages.Remove(page);
            }

            int num = 0;
            foreach (OuiJournalPage page in self.Pages)
            {
                page.PageIndex = num++;
            }
        }

        public static void InitializeMapLookup()
        {
            if (cleanNameToSidLookup != null) return;

            cleanNameToSidLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (AreaData area in AreaData.Areas)
            {
                if (area == null || string.IsNullOrEmpty(area.SID)) continue;

                string dialogKey = area.Name;

                string cleanName = Dialog.Clean(dialogKey);

                if (!string.IsNullOrWhiteSpace(cleanName))
                {
                    cleanNameToSidLookup.TryAdd(cleanName, area.SID);
                }
            }
        }

        private static Color GetLevelColor(string name)
        {
            string sid = cleanNameToSidLookup.TryGetValue(name, out string foundSid) ? foundSid : null;
            if(sid == null) { return Color.Black; }

            LevelCategory category = ArchipelagoMapper.getLevelCategory(sid, AreaMode.Normal);
            bool unlocked = ArchipelagoUtils.levelIsUnlocked(sid, AreaMode.Normal);
            if (ArchipelagoUtils.levelHasBCSides(sid))
            {
                unlocked = unlocked || ArchipelagoUtils.levelIsUnlocked(sid, AreaMode.BSide) || ArchipelagoUtils.levelIsUnlocked(sid, AreaMode.CSide);
            }
            bool included = ArchipelagoUtils.isIncludedInRandomizer(sid, AreaMode.Normal);

            return ArchipelagoUtils.levelIsUnlocked(sid, AreaMode.Normal) ? Color.Green : included ? Color.DarkRed : Color.DarkGray * 0.4f;
        }

        private void modCtor(On.Celeste.OuiJournalProgress.orig_ctor orig, OuiJournalProgress self, OuiJournal journal)
        {
            orig(self, journal);
            if (CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                OuiJournalPage.Table table = self.table;
                foreach (OuiJournalPage.Row row in table.rows)
                {
                    if (row.Entries.Count == 0) { continue; }
                    OuiJournalPage.TextCell cell = row.Entries[0] as OuiJournalPage.TextCell;
                    if (cell.text.Equals("PROGRESS") || cell.text.Equals("TOTALS")) continue;
                    cell.color = GetLevelColor(cell.text);
                }
            }
        }

        private static List<OuiJournalCollabProgressInLobby> modGeneratePages(orig_GeneratePages orig, OuiJournal journal, string levelSet, bool showOnlyDiscovered)
        {
            List<OuiJournalCollabProgressInLobby> pages = orig(journal, levelSet, showOnlyDiscovered);
            if (CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                foreach (OuiJournalCollabProgressInLobby page in pages)
                {
                    OuiJournalPage.Table table = TableField.GetValue(page) as OuiJournalPage.Table;
                    foreach (OuiJournalPage.Row row in table.rows)
                    {
                        if (row.Entries.Count == 0) { continue; }
                        OuiJournalPage.TextCell cell = row.Entries[0] as OuiJournalPage.TextCell;
                        cell.color = GetLevelColor(cell.text);
                    }
                    TableField.SetValue(page, table);
                }
                return pages;
            }
            else
            {
                return pages;
            }
        }
    }
}
