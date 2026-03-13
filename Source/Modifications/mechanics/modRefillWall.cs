using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Entities;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modRefillWall : IGameModification
    {
        private static Type RefillWallType;

        private static FieldInfo TwoDashesField;
        private static FieldInfo RespawnTimerField;

        private static Hook OnPlayerHook;
        private static Hook RenderHook;

        private delegate void orig_OnPlayer(RefillWall self, Player player);
        private delegate void orig_Render(RefillWall self);

        public override void Load()
        {
            RefillWallType = typeof(RefillWall);

            TwoDashesField = RefillWallType.GetField("twoDashes", BindingFlags.NonPublic | BindingFlags.Instance);
            RespawnTimerField = RefillWallType.GetField("respawnTimer", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo OnPlayerMethod = RefillWallType.GetMethod("OnPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo RenderMethod = RefillWallType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            OnPlayerHook = new Hook(OnPlayerMethod, typeof(modRefillWall).GetMethod(nameof(modOnPlayer), BindingFlags.NonPublic | BindingFlags.Static));
            RenderHook = new Hook(RenderMethod, typeof(modRefillWall).GetMethod(nameof(modRender), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            OnPlayerHook?.Dispose();
            OnPlayerHook = null;
        }

        private static void modOnPlayer(orig_OnPlayer orig, RefillWall self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                orig(self, player);
            }
        }

        private static void modRender(orig_Render orig, RefillWall self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !isActive(self))
            {
                RespawnTimerField.SetValue(self, 1.0f);
            }
            orig(self);
        }

        private static bool isActive(RefillWall self)
        {
            bool twoDashes = (bool)TwoDashesField.GetValue(self);

            return !twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_REFILL_WALL) ||
                twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_REFILL_WALL);
        }
    }
}
