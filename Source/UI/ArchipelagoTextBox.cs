using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    [Tracked]
    internal class ArchipelagoTextBox : Entity
    {
        private List<TextSegment> segments;
        private float totalWidth;
        private float displayDuration;
        private float alpha;

        private static readonly float TextScale = 0.6f;
        private static readonly float YPosition = 48f;
        private static readonly Color DefaultColor = Color.White;
        private static readonly Color OutlineColor = Color.Black;

        private struct TextSegment
        {
            public string Text;
            public Color Color;
        }

        public ArchipelagoTextBox(string input, float displayDuration = 3f) : base()
        {
            this.displayDuration = displayDuration;
            this.alpha = 0f;
            this.segments = ParseColoredText(input);
            this.totalWidth = MeasureTotalWidth();

            base.Tag = Tags.HUD | Tags.TransitionUpdate | Tags.Persistent;
            base.Depth = -100;

            base.Add(new Coroutine(DisplayRoutine()));
        }

        private static List<TextSegment> ParseColoredText(string input)
        {
            var result = new List<TextSegment>();
            Color currentColor = DefaultColor;
            int i = 0;

            while (i < input.Length)
            {
                int braceStart = input.IndexOf('{', i);
                if (braceStart == -1)
                {
                    string remaining = input.Substring(i);
                    if (remaining.Length > 0)
                        result.Add(new TextSegment { Text = remaining, Color = currentColor });
                    break;
                }

                if (braceStart > i)
                {
                    result.Add(new TextSegment { Text = input.Substring(i, braceStart - i), Color = currentColor });
                }

                int braceEnd = input.IndexOf('}', braceStart);
                if (braceEnd == -1)
                {
                    result.Add(new TextSegment { Text = input.Substring(braceStart), Color = currentColor });
                    break;
                }

                string tag = input.Substring(braceStart + 1, braceEnd - braceStart - 1);
                if (tag == "#")
                {
                    currentColor = DefaultColor;
                }
                else if (tag.Length == 7 && tag[0] == '#')
                {
                    if (uint.TryParse(tag.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hex))
                    {
                        currentColor = new Color((int)((hex >> 16) & 0xFF), (int)((hex >> 8) & 0xFF), (int)(hex & 0xFF));
                    }
                }

                i = braceEnd + 1;
            }

            return result;
        }

        private float MeasureTotalWidth()
        {
            float width = 0f;
            foreach (var seg in segments)
            {
                width += ActiveFont.Measure(seg.Text).X * TextScale;
            }
            return width;
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
            float startX = (1920f - totalWidth) / 2f;
            float x = startX;

            foreach (var seg in segments)
            {
                float segWidth = ActiveFont.Measure(seg.Text).X * TextScale;

                ActiveFont.Draw(
                    seg.Text,
                    new Vector2(x, YPosition),
                    Vector2.Zero,
                    Vector2.One * TextScale,
                    seg.Color * alpha,
                    1f,
                    OutlineColor * (alpha * 0.8f),
                    0f,
                    Color.Transparent
                );

                x += segWidth;
            }
        }
    }
}
