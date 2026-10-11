using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using static Celeste.Mod.CelesteArchipelago.ArchipelagoData.ArchipelagoManager;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    [Tracked]
    internal class ArchipelagoTextBox : Entity
    {
        private string message;
        private ArchipelagoMessage APMessage;
        private float displayDuration;
        private float alpha;
        private float textAlpha;
        private Color color;

        private float boxFullWidth;
        private float boxX;
        private float trueBoxX;
        private float boxY;
        private float boxWidth;
        private float boxHeight;
        private float textX;
        private float textY;
        private float textWidth;
        private float textHeight;
        private float trueTextScalar;
        private bool doCloseAnim;


        private static readonly float YPosition = 48f;
        private static readonly Color TextOutlineColor = Color.Black;
        private static readonly Color BoxInteriorColor = Color.Black * 0.5f;
        private static readonly Color BoxOutlineColor = new Color(50, 50, 50) * 0.9f;

        private static readonly float AnimationTime = 0.4f;
        private static readonly float StartingWidth = 50;
        private static readonly float DefaultTextScalar = 0.8f;

        public ArchipelagoTextBox() : base()
        {
            this.message = GetNextMessage(out ArchipelagoMessage? result) ? result?.Text : null;
            if(result == null)
            {
                RemoveSelf();
                return;
            }
            this.APMessage = result.Value;
            this.color = GetMessageColor(APMessage);
            CalculateBox();
            int queueSize = ArchipelagoManager.Instance.MessageQueue.Count;
            this.displayDuration = queueSize > 5 ? 1f : queueSize > 2 ? 1.5f : 2f;
            this.color = GetMessageColor(APMessage);
            this.alpha = 0f;
            this.textAlpha = 0f;
            this.boxWidth = StartingWidth;
            this.boxX = this.trueBoxX;
            base.Tag = Tags.HUD | Tags.TransitionUpdate | Tags.Persistent;
            base.Depth = -100;

            base.Add(new Coroutine(DisplayRoutine()));
        }

        private void CalculateBox()
        {
            this.trueTextScalar = DefaultTextScalar;

            this.textWidth = ActiveFont.Measure(message).X * DefaultTextScalar;
            this.boxFullWidth = textWidth + 20f;
            this.boxY = YPosition;
            if (boxFullWidth > Celeste.TargetWidth - 40f)
            {
                this.trueTextScalar = (Celeste.TargetWidth + boxFullWidth - 80f) / textWidth - 1;
                this.textWidth = ActiveFont.Measure(message).X * this.trueTextScalar;
                boxFullWidth = textWidth + 20f;
            }
            this.boxHeight = 100f * this.trueTextScalar;
            this.textHeight = ActiveFont.Measure(message).Y * this.trueTextScalar;
            this.textX = (Celeste.TargetWidth - textWidth - textHeight) / 2f;
            this.textY = YPosition + (boxHeight - textHeight) / 2;
            this.trueBoxX = textX - 10f;
        }

        private IEnumerator DisplayRoutine()
        {
            while (boxWidth < boxFullWidth || alpha < 1f)
            {
                alpha = Calc.Approach(alpha, 1f, Engine.RawDeltaTime * 4f);
                boxWidth = Calc.Approach(boxWidth, boxFullWidth, Engine.RawDeltaTime * (boxFullWidth - StartingWidth) / AnimationTime);
                yield return null;
            }

            if (APMessage.Type == ArchipelagoMessage.MessageType.ItemHint) this.displayDuration *= 2;

            while (textAlpha < 1f)
            {
                textAlpha = Calc.Approach(textAlpha, 1f, Engine.RawDeltaTime * 4f);
                yield return null;
            }
            if (color == Color.Orange) displayDuration *= 2;
            float timer = displayDuration;
            while (timer > 0f)
            {
                timer -= Engine.RawDeltaTime;
                yield return null;
            }
            while (textAlpha > 0f)
            {
                textAlpha = Calc.Approach(textAlpha, 0f, Engine.RawDeltaTime * 4f);
                yield return null;
            }


            while (GetNextMessage(out ArchipelagoMessage? result))
            {
                this.message = result?.Text;
                this.APMessage = result.Value;
                this.color = GetMessageColor(APMessage);
                int queueSize = ArchipelagoManager.Instance.MessageQueue.Count;
                this.displayDuration = queueSize > 5 ? 1f : queueSize > 2 ? 1.5f : 2f;
                CalculateBox();

                while (Math.Abs(boxWidth - boxFullWidth) > 0.01f || Math.Abs(trueBoxX - boxX) > 0.01f)
                {
                    float rightStepSize = 10f;
                    if(trueBoxX - boxX > 0.01f)
                    {
                        rightStepSize = 20f;
                    }
                    boxX = Calc.Approach(boxX, trueBoxX, 10f);
                    boxWidth = Calc.Approach(boxWidth, boxFullWidth, rightStepSize);
                    
                    yield return null;
                }

                if (result.Value.Type == ArchipelagoMessage.MessageType.ItemHint) this.displayDuration *= 2;

                while (textAlpha < 1f)
                {
                    textAlpha = Calc.Approach(textAlpha, 1f, Engine.RawDeltaTime * 4f);
                    yield return null;
                }
                if (color == Color.Orange) displayDuration *= 2;
                timer = displayDuration;
                while (timer > 0f)
                {
                    timer -= Engine.RawDeltaTime;
                    yield return null;
                }
                while (textAlpha > 0f)
                {
                    textAlpha = Calc.Approach(textAlpha, 0f, Engine.RawDeltaTime * 4f);
                    yield return null;
                }
            }

            while (boxWidth > StartingWidth || alpha > 0f)
            {
                alpha = Calc.Approach(alpha, 0f, Engine.RawDeltaTime * 4f);
                boxWidth = Calc.Approach(boxWidth, StartingWidth, Engine.RawDeltaTime * (boxFullWidth - StartingWidth) / AnimationTime);
                yield return null;
            }

            RemoveSelf();
        }

        public override void Render()
        {
            DrawRoundedRectWithOutline(new Vector2(boxX, YPosition), this.boxWidth, boxHeight, 10f, BoxInteriorColor * alpha, BoxOutlineColor * alpha);

            ActiveFont.Draw(
                message,
                new Vector2(textX, textY),
                Vector2.Zero,
                Vector2.One * trueTextScalar,
                color * textAlpha,
                0f,
                Color.Transparent,
                2f,
                TextOutlineColor * (textAlpha * 0.8f)
            );
        }

        private static Color GetMessageColor(ArchipelagoMessage message)
        {
            switch (message.Type)
            {
                case ArchipelagoMessage.MessageType.ItemReceive:
                    return Color.Salmon;
                case ArchipelagoMessage.MessageType.ItemSend:
                    return new Color(0xAF, 0x99, 0xEF);
                case ArchipelagoMessage.MessageType.ItemHint:
                    return Color.Orange;
                case ArchipelagoMessage.MessageType.Server:
                    return Color.LightGray;
                default:
                    return Color.White;
            }
        }

        private static bool ShouldShowMessage(ArchipelagoMessage message)
        {
            if (message.Type == ArchipelagoMessage.MessageType.Server)
            {
                return CelesteArchipelagoModule.Settings.ServerMessages;
            }
            return true;
        }

        private static bool GetNextMessage(out ArchipelagoMessage? message)
        {
            message = null;

            while (ArchipelagoManager.Instance.MessageQueue.TryDequeue(out ArchipelagoMessage result))
            {
                if (!ShouldShowMessage(result)) continue;
                else
                {
                    message = result;
                    return true;
                }
            }
            return false;
        }

        public static void DrawRoundedRectWithOutline(Vector2 position, float width, float height, float radius, Color fillColor, Color outlineColor, int thickness = 2)
        {
            int x = (int)MathF.Round(position.X);
            int y = (int)MathF.Round(position.Y);
            int w = (int)MathF.Round(width);
            int h = (int)MathF.Round(height);
            int r = (int)MathF.Round(Math.Min(radius, Math.Min(w, h) / 2f));

            int innerH = h - thickness * 2;
            int innerR = Math.Max(0, r - thickness);

            for (int row = 0; row < h; row++)
            {
                int outerInset = RowInset(row, h, r);

                int innerRow = row - thickness;
                bool inInterior = innerRow >= 0 && innerRow < innerH;

                if (!inInterior)
                {
                    Draw.Rect(x + outerInset, y + row, w - outerInset * 2, 1, outlineColor);
                    continue;
                }

                int innerInset = thickness + RowInset(innerRow, innerH, innerR);
                innerInset = Math.Max(innerInset, outerInset + 1);

                int borderW = innerInset - outerInset;

                Draw.Rect(x + outerInset, y + row, borderW, 1, outlineColor);
                Draw.Rect(x + innerInset, y + row, w - innerInset * 2, 1, fillColor);
                Draw.Rect(x + w - innerInset, y + row, borderW, 1, outlineColor);
            }
        }

        // Horizontal inset for a given row of a rounded rect of size h with corner radius r
        private static int RowInset(int row, int h, int r)
        {
            if (r <= 0) return 0;

            float dy;
            if (row < r) dy = r - row - 0.5f;
            else if (row >= h - r) dy = row - (h - r) + 0.5f;
            else return 0;

            float dx = MathF.Sqrt(MathF.Max(0, r * r - dy * dy));
            return r - (int)MathF.Round(dx);
        }
    }
}
