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

        private static readonly float TextScale = 0.6f;
        private static readonly Vector2 Position = new Vector2(32f, 48f);
        private static readonly Color TextColor = Color.White;
        private static readonly Color ShadowColor = Color.Black;

        public ArchipelagoTextBox(string input, float displayDuration = 3f) : base()
        {
            this.message = input;
            this.displayDuration = displayDuration;
            this.alpha = 0f;

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
            ActiveFont.Draw(
                message,
                Position,
                Vector2.Zero,
                Vector2.One * TextScale,
                TextColor * alpha,
                1f,
                ShadowColor * (alpha * 0.8f),
                0f,
                Color.Transparent
            );
        }
    }
}
