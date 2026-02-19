using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    [Tracked]
    internal class ArchipelagoTextBox : Entity
    {
        private string message;
        private float displayDuration;
        private float alpha;
        private Color color;

        private static readonly float TextScale = 0.6f;
        private static readonly float YPosition = 48f;
        private static readonly Color OutlineColor = Color.Black;

        public ArchipelagoTextBox(string input, float displayDuration = 3f, Color? color = null) : base()
        {
            this.message = input;
            this.displayDuration = displayDuration;
            this.alpha = 0f;
            this.color = color ?? Color.White;

            base.Tag = Tags.HUD | Tags.TransitionUpdate | Tags.Persistent;
            base.Depth = -100;

            base.Add(new Coroutine(DisplayRoutine()));
        }

        private IEnumerator DisplayRoutine()
        {
            while (alpha < 1f)
            {
                alpha = Calc.Approach(alpha, 1f, Engine.RawDeltaTime * 4f);
                yield return null;
            }

            float timer = displayDuration;
            while (timer > 0f)
            {
                timer -= Engine.RawDeltaTime;
                yield return null;
            }

            while (alpha > 0f)
            {
                alpha = Calc.Approach(alpha, 0f, Engine.RawDeltaTime * 4f);
                yield return null;
            }

            RemoveSelf();
        }

        public override void Render()
        {
            float textWidth = ActiveFont.Measure(message).X * TextScale;
            float x = (1920f - textWidth) / 2f;

            ActiveFont.Draw(
                message,
                new Vector2(x, YPosition),
                Vector2.Zero,
                Vector2.One * TextScale,
                color * alpha,
                1f,
                OutlineColor * (alpha * 0.8f),
                0f,
                Color.Transparent
            );
        }
    }
}
