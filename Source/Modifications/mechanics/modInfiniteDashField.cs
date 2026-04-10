using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.MaxHelpingHand.Entities;
using ExtendedVariants.Entities.Legacy;
using ExtendedVariants.Module;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Triggers;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modInfiniteDashField : IGameModification
    {
        private static Type InfiniteDashTriggerType;

        private static Hook HookEnter;
        private static Hook HookLeave;
        private static Hook HookStay;
        private delegate void orig_OnEnter(InfiniteDashTrigger self, Player player);
        private delegate void orig_OnLeave(InfiniteDashTrigger self, Player player);
        private delegate void orig_Onstay(InfiniteDashTrigger self, Player player);

        public override void Load()
        {
            InfiniteDashTriggerType = typeof(InfiniteDashTrigger);

            HookEnter = new Hook(InfiniteDashTriggerType.GetMethod("OnEnter", BindingFlags.Public | BindingFlags.Instance),
                typeof(modInfiniteDashField).GetMethod(nameof(modOnEnter), BindingFlags.Static | BindingFlags.NonPublic));
            HookStay = new Hook(InfiniteDashTriggerType.GetMethod("OnStay", BindingFlags.Public | BindingFlags.Instance),
                typeof(modInfiniteDashField).GetMethod(nameof(modOnEnter), BindingFlags.Static | BindingFlags.NonPublic));
            HookLeave = new Hook(InfiniteDashTriggerType.GetMethod("OnLeave", BindingFlags.Public | BindingFlags.Instance),
                typeof(modInfiniteDashField).GetMethod(nameof(modOnLeave), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            HookEnter?.Dispose();
            HookLeave?.Dispose();
            HookStay?.Dispose();
            HookEnter = null;
            HookLeave = null;
            HookStay = null;
        }

        private static void modOnEnter(orig_OnEnter orig, InfiniteDashTrigger self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                orig(self, player);
            }
        }

        private static void modOnLeave(orig_OnLeave orig, InfiniteDashTrigger self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                orig(self, player);
            }
        }

        private static void modOnStay(orig_Onstay orig, InfiniteDashTrigger self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                orig(self, player);
            }
        }

        private static bool isActive(InfiniteDashTrigger self)
        {
            return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.INFINITE_DASH_FIELD) && isInCMB();
        }

        private static bool isInCMB()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/5-Grandmaster/DeathKontrol") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("c2_11-DeathKontrol");
        }
    }
}
