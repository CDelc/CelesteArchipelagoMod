using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modTrafficBlock : IGameModification
    {
        private static readonly FieldInfo ropeColorField =
            typeof(ZipMover).GetField("ropeColor", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo ropeLightColorField =
            typeof(ZipMover).GetField("ropeLightColor", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void Load()
        {
            On.Celeste.ZipMover.Awake += ZipMover_Awake;
        }

        public override void Unload()
        {
            On.Celeste.ZipMover.Awake -= ZipMover_Awake;
        }

        private void ZipMover_Awake(On.Celeste.ZipMover.orig_Awake orig, ZipMover self, Scene scene)
        {
            orig(self, scene);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave) return;

            ApplyRedTint(self);
        }

        private static void ApplyRedTint(ZipMover zipMover)
        {
            if (ropeColorField != null)
            {
                Color current = (Color)ropeColorField.GetValue(zipMover);
                ropeColorField.SetValue(zipMover, ShiftToRed(current));
            }

            if (ropeLightColorField != null)
            {
                Color current = (Color)ropeLightColorField.GetValue(zipMover);
                ropeLightColorField.SetValue(zipMover, ShiftToRed(current));
            }

            foreach (Component component in zipMover.Components)
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
