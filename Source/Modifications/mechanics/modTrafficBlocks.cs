using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CommunalHelper.Entities;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using VivHelper.Entities.CurvedStuff;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modTrafficBlocks : IGameModification
    {

        private static Type VanillaZipMoverType;
        private static Type CustomCurvedZipMoverType;
        private static Type ConnectedZipMoverType;
        private static Type CustomZipMoverType;

        private static Hook VanillaUpdateHook;
        private static Hook CustomUpdateHook;
        private static Hook ConnectedUpdateHook;
        private static Hook CustomFrostUpdateHook;
        private static Hook VanillaRenderHook;
        private static Hook CustomRenderHook;
        private static Hook ConnectedRenderHook;
        private static Hook CustomFrostRenderHook;

        private delegate void orig_Render(Solid self);
        private delegate void orig_Update(Solid self);
        
        public override void Load()
        {
            VanillaZipMoverType = typeof(ZipMover);
            CustomCurvedZipMoverType = typeof(CustomCurvedZipMover);
            ConnectedZipMoverType = typeof(ConnectedZipMover);
            CustomZipMoverType = typeof(CustomZipMover);

            MethodInfo VanillaUpdateMethod = VanillaZipMoverType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo VanillaRenderMethod = VanillaZipMoverType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);

            MethodInfo CustomUpdateMethod = CustomCurvedZipMoverType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo CustomRenderMethod = CustomCurvedZipMoverType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);

            MethodInfo ConnectedUpdateMethod = ConnectedZipMoverType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo ConnectedRenderMethod = ConnectedZipMoverType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);

            MethodInfo CustomFrostUpdateMethod = CustomZipMoverType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo CustomFrostRenderMethod = CustomZipMoverType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);

            VanillaUpdateHook = new Hook(VanillaUpdateMethod, typeof(modTrafficBlocks).GetMethod(nameof(modZipMover_Update), BindingFlags.NonPublic | BindingFlags.Static));
            VanillaRenderHook = new Hook(VanillaRenderMethod, typeof(modTrafficBlocks).GetMethod(nameof(modZipMover_Render), BindingFlags.NonPublic | BindingFlags.Static));

            CustomUpdateHook = new Hook(CustomUpdateMethod, typeof(modTrafficBlocks).GetMethod(nameof(modZipMover_Update), BindingFlags.NonPublic | BindingFlags.Static));
            CustomRenderHook = new Hook(CustomRenderMethod, typeof(modTrafficBlocks).GetMethod(nameof(modZipMover_Render), BindingFlags.NonPublic | BindingFlags.Static));

            ConnectedUpdateHook = new Hook(ConnectedUpdateMethod, typeof(modTrafficBlocks).GetMethod(nameof(modZipMover_Update), BindingFlags.NonPublic | BindingFlags.Static));
            ConnectedRenderHook = new Hook(ConnectedUpdateMethod, typeof(modTrafficBlocks).GetMethod(nameof(modZipMover_Render), BindingFlags.NonPublic | BindingFlags.Static));

            CustomFrostUpdateHook = new Hook(CustomFrostUpdateMethod, typeof(modTrafficBlocks).GetMethod(nameof(modZipMover_Update), BindingFlags.NonPublic | BindingFlags.Static));
            CustomFrostRenderHook = new Hook(CustomFrostRenderMethod, typeof(modTrafficBlocks).GetMethod(nameof(modZipMover_Render), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            VanillaUpdateHook?.Dispose();
            CustomUpdateHook?.Dispose();
            VanillaRenderHook?.Dispose();
            CustomRenderHook?.Dispose();
            ConnectedUpdateHook?.Dispose();
            ConnectedRenderHook?.Dispose();
            CustomFrostUpdateHook?.Dispose();
            CustomFrostRenderHook?.Dispose();

            VanillaUpdateHook = null;
            CustomUpdateHook = null;
            VanillaRenderHook = null;
            CustomRenderHook = null;
            ConnectedUpdateHook = null;
            ConnectedRenderHook = null;
            CustomFrostUpdateHook = null;
            CustomFrostRenderHook = null;
        }

        private static void modZipMover_Render(orig_Render orig, Solid self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return;
            }

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TRAFFIC_BLOCKS))
            {
                Constants.DrawDisabledRect(self.Collider);
            }
        }

        private static void modZipMover_Update(orig_Update orig, Solid self)
        {
            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TRAFFIC_BLOCKS) || !CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
            }
        }
    }
}
