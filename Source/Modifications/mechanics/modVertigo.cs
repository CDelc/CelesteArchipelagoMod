using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.Helpers;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Entities;
using VivHelper.Triggers;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modVertigo : IGameModification
    {

        private static Type TeleportTriggerType;
        private static Type TeleportTrigger1WayType;

        private static Hook HookTeleportEnter;
        private static Hook HookTeleport1WayEnter;

        private static Hook HookTeleportLeave;
        private static Hook HookTeleport1WayLeave;

        private delegate void orig_EnterLeave(InstantTeleportTrigger self, Player player);
        private delegate void orig_EnterLeave1Way(InstantTeleportTrigger1Way self, Player player);

        public override void Load()
        {
            TeleportTriggerType = typeof(InstantTeleportTrigger);
            TeleportTrigger1WayType = typeof(InstantTeleportTrigger1Way);

            MethodInfo onEnterMethod = TeleportTriggerType.GetMethod("OnEnter", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo onEnterMethod1Way = TeleportTrigger1WayType.GetMethod("OnLeave", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo onLeaveMethod = TeleportTriggerType.GetMethod("OnEnter", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo onLeaveMethod1Way = TeleportTrigger1WayType.GetMethod("OnLeave", BindingFlags.Public | BindingFlags.Instance);

            HookTeleportEnter = new Hook(onEnterMethod, typeof(modVertigo).GetMethod(nameof(modBehavior), BindingFlags.NonPublic | BindingFlags.Static));
            HookTeleport1WayEnter = new Hook(onEnterMethod1Way, typeof(modVertigo).GetMethod(nameof(modBehavior1Way), BindingFlags.NonPublic | BindingFlags.Static));
            HookTeleportLeave = new Hook(onLeaveMethod, typeof(modVertigo).GetMethod(nameof(modBehavior), BindingFlags.NonPublic | BindingFlags.Static));
            HookTeleport1WayLeave = new Hook(onLeaveMethod1Way, typeof(modVertigo).GetMethod(nameof(modBehavior1Way), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            HookTeleportEnter?.Dispose();
            HookTeleport1WayEnter?.Dispose();
            HookTeleportLeave?.Dispose();
            HookTeleport1WayLeave?.Dispose();

            HookTeleportEnter = null;
            HookTeleport1WayEnter = null;
            HookTeleportLeave = null;
            HookTeleport1WayLeave = null;
        }


        private static void modBehavior(orig_EnterLeave orig, InstantTeleportTrigger self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || !isInVertigo())
            {
                orig(self, player);
                return;
            }

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.VERTIGO_LINKED_TELEPORT)) return;
            orig(self, player);
        }

        private static void modBehavior1Way(orig_EnterLeave1Way orig, InstantTeleportTrigger1Way self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || !isInVertigo())
            {
                orig(self, player);
                return;
            }

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.VERTIGO_LINKED_TELEPORT)) return;
            orig(self, player);
        }

        public static bool isInVertigo()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/2-Intermediate/Evilleafy") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("cp1-3-Evilleafy");
        }
    }
}
