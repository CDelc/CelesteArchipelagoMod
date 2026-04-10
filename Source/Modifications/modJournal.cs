using Celeste.Mod.CelesteArchipelago.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modJournal : IGameModification
    {
        public override void Load()
        {
            On.Celeste.OuiJournal.Enter += modEnter;
        }

        public override void Unload()
        {
            On.Celeste.OuiJournal.Enter -= modEnter;
        }

        private static IEnumerator modEnter(On.Celeste.OuiJournal.orig_Enter orig, OuiJournal self, Oui from)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                yield return orig(self, from);
            }

            else
            {
                IEnumerator orig_return = orig(self, from);
                self.Pages.Clear();
                self.Pages.Add(new OuiJournalCover(self));
                self.Pages.Add(new OuiArchipelagoJournal(self));
                yield return orig_return;
            }
        }


    }
}
