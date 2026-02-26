using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modLoopBlock : IGameModification
    {
        private static Type loopBlockType;
        private static FieldInfo colorField;

        private static Hook hookUpdate;
        private static Hook hookRender;
        private static Hook hookOnDashed;

        private delegate void orig_Update(Entity self);
        private delegate void orig_Render(Entity self);
        private delegate DashCollisionResults orig_OnDashed(Entity self, Player player, Vector2 dir);

        public override void Load()
        {
            loopBlockType = FindLoopBlockType();
            if (loopBlockType == null) return;

            colorField = loopBlockType.GetField("color", BindingFlags.NonPublic | BindingFlags.Instance);

            var updateMethod = loopBlockType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            if (updateMethod != null)
                hookUpdate = new Hook(updateMethod, typeof(modLoopBlock).GetMethod(nameof(ModUpdate), BindingFlags.NonPublic | BindingFlags.Static));

            var renderMethod = loopBlockType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            if (renderMethod != null)
                hookRender = new Hook(renderMethod, typeof(modLoopBlock).GetMethod(nameof(ModRender), BindingFlags.NonPublic | BindingFlags.Static));

            var onDashedMethod = loopBlockType.GetMethod("OnDashed", BindingFlags.NonPublic | BindingFlags.Instance);
            if (onDashedMethod != null)
                hookOnDashed = new Hook(onDashedMethod, typeof(modLoopBlock).GetMethod(nameof(ModOnDashed), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            hookUpdate?.Dispose();
            hookRender?.Dispose();
            hookOnDashed?.Dispose();
            hookUpdate = null;
            hookRender = null;
            hookOnDashed = null;
        }

        private static Type FindLoopBlockType()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType("Celeste.Mod.StrawberryJam2021.Entities.LoopBlock");
                if (type != null) return type;
            }
            return null;
        }

        private static void ModUpdate(orig_Update orig, Entity self)
        {
            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.LOOP_BLOCK) || !CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
            }
        }

        private static void ModRender(orig_Render orig, Entity self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.LOOP_BLOCK))
            {
                Color origColor = (Color)colorField.GetValue(self);
                colorField.SetValue(self, Color.Gray);
                orig(self);
                colorField.SetValue(self, origColor);
            }
            else
            {
                orig(self);
            }
        }

        private static DashCollisionResults ModOnDashed(orig_OnDashed orig, Entity self, Player player, Vector2 dir)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.LOOP_BLOCK))
            {
                return orig(self, player, dir);
            }
            return DashCollisionResults.NormalCollision;
        }
    }
}