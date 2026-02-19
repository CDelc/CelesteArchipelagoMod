using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modTrafficBlock : IGameModification
    {
        private static readonly FieldInfo fillField =
            typeof(CrushBlock).GetField("fill", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void Load()
        {
            On.Celeste.CrushBlock.Awake += CrushBlock_Awake;
        }

        public override void Unload()
        {
            On.Celeste.CrushBlock.Awake -= CrushBlock_Awake;
        }

        private void CrushBlock_Awake(On.Celeste.CrushBlock.orig_Awake orig, CrushBlock self, Scene scene)
        {
            orig(self, scene);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave) return;

            ApplyRedTint(self);
        }

        private static void ApplyRedTint(CrushBlock block)
        {
            if (fillField != null)
            {
                Color currentFill = (Color)fillField.GetValue(block);
                fillField.SetValue(block, ShiftToRed(currentFill));
            }

            foreach (Component component in block.Components)
            {
                if (component is Image image)
                {
                    image.Color = ShiftToRed(image.Color);
                }
            }
        }

        private static Color ShiftToRed(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float a = color.A / 255f;

            return new Color(
                MathHelper.Clamp(r + 0.3f, 0f, 1f),
                g * 0.5f,
                b * 0.5f,
                a
            );
        }
    }
}
