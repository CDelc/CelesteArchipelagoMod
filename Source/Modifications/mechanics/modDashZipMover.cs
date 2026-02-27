using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modDashZipMover : IGameModification
    {

        private static Type DashZipMoverType;
        private static FieldInfo streetLightField;

        private static Hook hookOnDashed;
        private static Hook hookRender;

        static Color originalColor;
        static bool setColor = false;

        private delegate DashCollisionResults orig_OnDashed(Entity self, Player player, Microsoft.Xna.Framework.Vector2 dir);
        private delegate void orig_Render(Entity self);

        public override void Load()
        {
            DashZipMoverType = CelesteArchipelagoModule.FindType("Celeste.Mod.StrawberryJam2021.Entities.DashZipMover");
            streetLightField = DashZipMoverType.GetField("streetlight", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo onDashedMethod = DashZipMoverType.GetMethod("OnDashed", BindingFlags.Public | BindingFlags.Instance);
            hookOnDashed = new Hook(onDashedMethod, typeof(modDashZipMover).GetMethod(nameof(modOnDashed), BindingFlags.NonPublic | BindingFlags.Static));

            MethodInfo renderMethod = DashZipMoverType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            hookRender = new Hook(renderMethod, typeof(modDashZipMover).GetMethod(nameof(modRender), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            hookOnDashed?.Dispose();
            hookOnDashed = null;

            hookRender?.Dispose();
            hookRender = null;
        }

        private static DashCollisionResults modOnDashed(orig_OnDashed orig, Entity self, Player player, Microsoft.Xna.Framework.Vector2 dir)
        {
            if(!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_ZIP_MOVER))
            {
                return orig(self, player, dir);
            }

            return DashCollisionResults.NormalCollision;
        }

        private static void modRender(orig_Render orig, Entity self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return;
            }

            Sprite streetlight = (Sprite)streetLightField.GetValue(self);

            if (!setColor)
            {
                originalColor = streetlight.Color;
                setColor = true;
            }

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_ZIP_MOVER))
            {
                streetlight.Color = Color.Purple;
            }
            else
            {
                streetlight.Color = originalColor;
            }
        }
    }
}
