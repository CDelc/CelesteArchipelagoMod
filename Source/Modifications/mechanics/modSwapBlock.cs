using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.StrawberryJam2021.Entities;
using FrostHelper;
using FrostHelper.Helpers;
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
    internal class modSwapBlock : IGameModification
    {

        private static Type ToggleSwapBlockType;
        private static Type ToggleSwapBlockCanyonType;
        private static Type ToggleSwapBlockSJType;

        private static Hook HookRender;
        private static Hook HookOnDash;
        private static Hook HookRenderCanyon;
        private static Hook HookOnDashCanyon;
        private static Hook HookRenderSJ;
        private static Hook HookOnDashSJ;

        private delegate void orig_Render(Solid self);
        private delegate void orig_OnDash(Solid self, Vector2 direction);

        public override void Load()
        {
            ToggleSwapBlockType = typeof(FrostHelper.ToggleSwapBlock);
            ToggleSwapBlockCanyonType = CelesteArchipelagoModule.FindType("Celeste.Mod.CanyonHelper.ToggleSwapBlock");
            ToggleSwapBlockSJType = typeof(StrawberryJam2021.Entities.ToggleSwapBlock);

            MethodInfo renderMethod = ToggleSwapBlockType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo OnDashMethod = ToggleSwapBlockType.GetMethod("OnDash", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo renderMethodCanyon = ToggleSwapBlockCanyonType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo OnDashMethodCanyon = ToggleSwapBlockCanyonType.GetMethod("OnPlayerDashed", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo renderMethodSJ = ToggleSwapBlockSJType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo OnDashMethodSJ = ToggleSwapBlockSJType.GetMethod("OnPlayerDashed", BindingFlags.NonPublic | BindingFlags.Instance);

            HookRender = new Hook(renderMethod, typeof(modSwapBlock).GetMethod(nameof(modToggleSwapBlock_Render), BindingFlags.NonPublic | BindingFlags.Static));
            HookOnDash = new Hook(OnDashMethod, typeof(modSwapBlock).GetMethod(nameof(modToggleSwapBlock_OnDash), BindingFlags.NonPublic | BindingFlags.Static));

            HookRenderCanyon = new Hook(renderMethodCanyon, typeof(modSwapBlock).GetMethod(nameof(modToggleSwapBlock_Render), BindingFlags.NonPublic | BindingFlags.Static));
            HookOnDashCanyon = new Hook(OnDashMethodCanyon, typeof(modSwapBlock).GetMethod(nameof(modToggleSwapBlock_OnDash), BindingFlags.NonPublic | BindingFlags.Static));

            HookRenderSJ = new Hook(renderMethodSJ, typeof(modSwapBlock).GetMethod(nameof(modToggleSwapBlock_Render), BindingFlags.NonPublic | BindingFlags.Static));
            HookOnDashSJ = new Hook(OnDashMethodSJ, typeof(modSwapBlock).GetMethod(nameof(modToggleSwapBlock_OnDash), BindingFlags.NonPublic | BindingFlags.Static));

            On.Celeste.SwapBlock.Render += modSwapBlock_Render;
            On.Celeste.SwapBlock.OnDash += modSwapBlock_OnDash;
        }

        public override void Unload()
        {
            On.Celeste.SwapBlock.Render -= modSwapBlock_Render;
            On.Celeste.SwapBlock.OnDash -= modSwapBlock_OnDash;

            HookRender?.Dispose();
            HookOnDash?.Dispose();
            HookRenderCanyon?.Dispose();
            HookOnDashCanyon?.Dispose();
            HookRenderSJ?.Dispose();
            HookOnDashSJ?.Dispose();

            HookRender = null;
            HookOnDash = null;
            HookRenderCanyon = null;
            HookOnDashCanyon = null;
            HookRenderSJ = null;
            HookOnDashSJ = null;
        }

        private static void modSwapBlock_Render(On.Celeste.SwapBlock.orig_Render orig, SwapBlock self)
        {
            orig(self);
            if (!SwapBlockIsActive() && CelesteArchipelagoModule.shouldModMechanics)
            {
                Constants.DrawDisabledRect(self.Collider);
            }
        }

        private static void modSwapBlock_OnDash(On.Celeste.SwapBlock.orig_OnDash orig, SwapBlock self, Vector2 direction)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || SwapBlockIsActive())
            {
                orig(self, direction);
            }
        }

        private static void modToggleSwapBlock_Render(orig_Render orig, Solid self)
        {
            orig(self);

            if (CelesteArchipelagoModule.shouldModMechanics && !ToggleSwapBlockIsActive())
            {
                Constants.DrawDisabledRect(self.Collider, isInToggleTheory() ? Color.Black * 0.8f : Constants.DisabledColor);
            }

        }

        private static void modToggleSwapBlock_OnDash(orig_OnDash orig, Solid self, Vector2 direction)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ToggleSwapBlockIsActive())
            {
                orig(self, direction);
            }
        }

        private static bool SwapBlockIsActive()
        {
            return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SWAP_BLOCK) && !isNelumbo() ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_DRUM) && isNelumbo();
        }

        private static bool ToggleSwapBlockIsActive()
        {
            return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TOGGLE_SWAP_BLOCK) && !isNelumbo() ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TOGGLE_DRUM) && isNelumbo();
        }

        private static bool isInToggleTheory()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID == "StrawberryJam2021/3-Advanced/Citrea" ||
                SaveData.Instance.CurrentSession_Safe.Level == "heartside_citrea";
        }

        private static bool isNelumbo()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/5-Grandmaster/tofu") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("e1_06-tofu");
        }
    }
}
