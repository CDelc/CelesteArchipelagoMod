using Celeste.Mod.AdventureHelper.Entities;
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
    internal class modLinkedZipMover : IGameModification
    {

        private static Type LinkedZipMoverNoReturnType;
        private static Type LinkedZipMoverType;

        private static Hook HookUpdate;
        private static Hook HookUpdateNoReturn;

        private delegate void orig_Update(LinkedZipMover self);
        private delegate void orig_UpdateNoReturn(LinkedZipMoverNoReturn self);

        private static Hook HookRender;
        private static Hook HookRenderNoReturn;

        private delegate void orig_Render(LinkedZipMover self);
        private delegate void orig_RenderNoReturn(LinkedZipMoverNoReturn self);


        public override void Load()
        {
            LinkedZipMoverNoReturnType = typeof(AdventureHelper.Entities.LinkedZipMoverNoReturn);
            LinkedZipMoverType = typeof(AdventureHelper.Entities.LinkedZipMover);

            MethodInfo updateMethod = LinkedZipMoverType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo updateMethodNoReturn = LinkedZipMoverNoReturnType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);

            HookUpdate = new Hook(updateMethod, typeof(modLinkedZipMover).GetMethod(nameof(modUpdate), BindingFlags.NonPublic | BindingFlags.Static));
            HookUpdateNoReturn = new Hook(updateMethodNoReturn, typeof(modLinkedZipMover).GetMethod(nameof(modUpdateNoReturn), BindingFlags.NonPublic | BindingFlags.Static));

            MethodInfo renderMethod = LinkedZipMoverType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo renderMethodNoReturn = LinkedZipMoverNoReturnType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);

            HookRender = new Hook(renderMethod, typeof(modLinkedZipMover).GetMethod(nameof(modRender), BindingFlags.NonPublic | BindingFlags.Static));
            HookRenderNoReturn = new Hook(renderMethodNoReturn, typeof(modLinkedZipMover).GetMethod(nameof(modRenderNoReturn), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            HookUpdate?.Dispose();
            HookUpdateNoReturn?.Dispose();
            HookRender?.Dispose();
            HookRenderNoReturn?.Dispose();

            HookUpdate = null;
            HookUpdateNoReturn = null;
            HookRender = null;
            HookRenderNoReturn = null;
        }

        private static void modUpdate(orig_Update orig, LinkedZipMover self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self.ColorCode))
            {
                orig(self);
            }
        }

        private static void modUpdateNoReturn(orig_UpdateNoReturn orig, LinkedZipMoverNoReturn self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self.ColorCode))
            {
                orig(self);
            }
        }

        private static void modRender(orig_Render orig, LinkedZipMover self)
        {
            orig(self);
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self.ColorCode))
            {
                return;
            }
            else
            {
                Constants.DrawDisabledRect(self.Collider);
            }
        }

        private static void modRenderNoReturn(orig_RenderNoReturn orig, LinkedZipMoverNoReturn self)
        {
            orig(self);
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self.ColorCode))
            {
                return;
            }
            else
            {
                Constants.DrawDisabledRect(self.Collider);
            }
        }

        private static bool isActive(string ColorCode)
        {
            return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.ORANGE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("ff3a0a") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.ORANGE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("ffaa00") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("105aff") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("0000ff") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("49a6e9") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TORQUOISE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("00ffff") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("5aef2d") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("00ff00") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("ffff00") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MAGENTA_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("ff00ff") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("ff0000") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("333399") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("aa00ff") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("aa00ff") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.WHITE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("a6a47c") ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.WHITE_LINKED_TRAFFIC_BLOCK) && ColorCode.Equals("ffffff");
        }
    }
}
