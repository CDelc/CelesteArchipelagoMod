using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modDashZipMover : IGameModification
    {

        private static Type DashZipMoverType;
        private static FieldInfo streetLightField;

        private static Hook hookOnDashed;
        private static Hook hookRender;

        private delegate DashCollisionResults orig_OnDashed(Entity self, Player player, Microsoft.Xna.Framework.Vector2 dir);
        private delegate void orig_Render(Entity self);

        public override void Load()
        {
            DashZipMoverType = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.Entities.StrawberryJam.DashZipMover");
            if (DashZipMoverType == null) return;

            streetLightField = DashZipMoverType.GetField("streetlight", BindingFlags.NonPublic | BindingFlags.Instance);

            var onDashedMethod = DashZipMoverType.GetMethod("OnDashed", BindingFlags.Public | BindingFlags.Instance);
            if (onDashedMethod != null)
                hookOnDashed = new Hook(onDashedMethod, typeof(modDashZipMover).GetMethod(nameof(modOnDashed), BindingFlags.NonPublic | BindingFlags.Static));

            var renderMethod = DashZipMoverType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            if (renderMethod != null)
                hookRender = new Hook(renderMethod, typeof(modDashZipMover).GetMethod(nameof(modRender), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            hookOnDashed?.Dispose();
            hookRender?.Dispose();
            hookOnDashed = null;
            hookRender = null;
        }

        private static DashCollisionResults modOnDashed(orig_OnDashed orig, Entity self, Player player, Microsoft.Xna.Framework.Vector2 dir)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_ZIP_MOVER))
            {
                return orig(self, player, dir);
            }

            return DashCollisionResults.NormalCollision;
        }

        private static void modRender(orig_Render orig, Entity self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_ZIP_MOVER))
            {
                Sprite streetlight = (Sprite)streetLightField.GetValue(self);
                Color origColor = streetlight.Color;
                streetlight.Color = Color.DarkRed;
                orig(self);
                streetlight.Color = origColor;
            }
            else
            {
                orig(self);
            }
        }
    }
}
