using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    internal class OuiArchipelagoJournal : OuiJournalPage
    {

        private Table table;

        public OuiArchipelagoJournal(OuiJournal journal) : base(journal)
        {
            PageTexture = "page";
            table = new Table().AddColumn(new TextCell(Dialog.Clean("journal_progress"), new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f)).AddColumn(new TextCell("Locked Status", new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f));
            foreach ((string SID, AreaMode mode) in ArchipelagoMapper.levelSIDToID.Keys)
            {
                Color textColor;
                bool isInGame = ArchipelagoMapper.levelsEnabledOnCategory(ArchipelagoMapper.getLevelCategory(SID, mode));
                bool isUnlocked = CelesteArchipelagoModule.SaveData.LevelUnlocks.Contains((SID, mode));
                if (!isInGame)
                {
                    textColor = Color.DarkGray * 0.5f;
                }
                else if(!isUnlocked)
                {
                    textColor = Color.DarkRed;
                }
                else
                {
                    textColor = TextColor;
                }
                Row row = table.AddRow().Add(new TextCell(SID, new Vector2(1f, 0.5f), 0.6f, textColor))
                    .Add(new TextCell(!isInGame ? "EXCLUDED" : !isUnlocked ? "LOCKED" : "AVAILABLE", new Vector2(1f, 0.5f), 0.6f, textColor));
            }
        }
    }
}
