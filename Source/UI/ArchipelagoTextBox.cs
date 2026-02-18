using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    internal class ArchipelagoTextBox : MiniTextbox
    {
        [Tracked]
        public ArchipelagoTextBox(string input) : base("placeholder")
        {
            base.Tag = Tags.HUD | Tags.TransitionUpdate | Tags.Persistent;
            this.portraitSize = 0f;
            this.box = GFX.Portraits["textbox/default"];
            this.text = FancyText.Parse(input, (int)(1688f - 32f), 2, 1f, null, null);

            base.Add(this.routine = new Coroutine(this.Routine(), true));
            this.routine.UseRawDeltaTime = true;

            TransitionListener listener = base.Get<TransitionListener>();
            if (listener != null)
            {
                base.Remove(listener);
            }
        }
    }
}
