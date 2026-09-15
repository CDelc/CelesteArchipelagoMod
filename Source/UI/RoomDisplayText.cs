using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    internal class RoomDisplayText : Entity
    {
        private string roomName;
        private float displayDuration;
        private static readonly Color color = Color.White;

        private float alpha = 1f;

        private static readonly float TextScale = 1f;
        private static readonly float YPosition = Celeste.TargetHeight - 100f;
        private static readonly float XPosition = 65f;
        private static readonly Color OutlineColor = Color.Black;

        public RoomDisplayText(string roomName, float displayDuration = 2f) : base()
        {
            this.roomName = roomName;
            this.displayDuration = displayDuration;

            base.Tag = Tags.HUD | Tags.TransitionUpdate | Tags.Persistent;
            base.Depth = -100;

            base.Add(new Coroutine(DisplayRoutine()));
        }

        private IEnumerator DisplayRoutine()
        {
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
                roomName,
                new Vector2(XPosition, YPosition),
                Vector2.Zero,
                Vector2.One * TextScale,
                color * alpha,
                1.5f,
                OutlineColor * (alpha * 0.8f),
                2f,
                Color.Black
            );
        }
    }
}
