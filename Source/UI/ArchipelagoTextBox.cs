using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    [Tracked]
    internal class ArchipelagoTextBox : MiniTextbox
    {
        private float displayDuration;

        public ArchipelagoTextBox(string input, float displayDuration = 3f) : base("placeholder")
        {
            this.displayDuration = displayDuration;
            base.Tag = Tags.HUD | Tags.TransitionUpdate | Tags.Persistent;
            this.portraitSize = 0f;
            this.box = GFX.Portraits["textbox/default"];
            this.text = FancyText.Parse(input, (int)(1688f - 32f), 2, 1f, null, null);

            base.Add(this.routine = new Coroutine(this.Routine(), true));
            if (this.routine != null)
            {
                base.Remove(this.routine);
            }

            base.Add(this.routine = new Coroutine(this.DisplayRoutine(), true));

            TransitionListener listener = base.Get<TransitionListener>();
            if (listener != null)
            {
                base.Remove(listener);
            }
        }

        private IEnumerator DisplayRoutine()
        {
            while (this.ease < 1f)
            {
                this.ease = Calc.Approach(this.ease, 1f, Engine.RawDeltaTime * 4f);
                yield return null;
            }

            float timer = this.displayDuration;
            while (timer > 0f)
            {
                timer -= Engine.RawDeltaTime;
                yield return null;
            }

            while (this.ease > 0f)
            {
                this.ease = Calc.Approach(this.ease, 0f, Engine.RawDeltaTime * 4f);
                yield return null;
            }

            RemoveSelf();
        }
    }
}
