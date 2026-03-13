using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.GravityHelper.Triggers;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modGravityTrigger : IGameModification
    {

        private static Hook hookHandleOnEnter;
        private static Hook hookHandleOnStay;
        private static Hook hookHandleOnLeave;

        private static Type GravityTriggerType;

        private delegate void orig_HandleOnEnterOrStayOrLeave(GravityTrigger self, Player player);

        public override void Load()
        {
            GravityTriggerType = CelesteArchipelagoModule.FindType("Celeste.Mod.GravityHelper.Triggers.GravityTrigger");

            MethodInfo onEnterMethod = GravityTriggerType.GetMethod("HandleOnEnter", BindingFlags.NonPublic | BindingFlags.Instance);
            hookHandleOnEnter = new Hook(onEnterMethod, typeof(modGravityTrigger).GetMethod(nameof(modHandleOnEnterOrStayOrLeave), BindingFlags.NonPublic | BindingFlags.Static));

            MethodInfo onStayMethod = GravityTriggerType.GetMethod("HandleOnStay", BindingFlags.NonPublic | BindingFlags.Instance);
            hookHandleOnStay = new Hook(onStayMethod, typeof(modGravityTrigger).GetMethod(nameof(modHandleOnEnterOrStayOrLeave), BindingFlags.NonPublic | BindingFlags.Static));

            MethodInfo onLeaveMethod = GravityTriggerType.GetMethod("HandleOnLeave", BindingFlags.NonPublic | BindingFlags.Instance);
            hookHandleOnLeave = new Hook(onLeaveMethod, typeof(modGravityTrigger).GetMethod(nameof(modHandleOnEnterOrStayOrLeave), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            hookHandleOnEnter?.Dispose();
            hookHandleOnEnter = null;

            hookHandleOnStay?.Dispose();
            hookHandleOnStay = null;

            hookHandleOnLeave?.Dispose();
            hookHandleOnLeave = null;
        }

        private static void modHandleOnEnterOrStayOrLeave(orig_HandleOnEnterOrStayOrLeave orig, GravityTrigger self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GRAVITY_FIELD))
            {
                orig(self, player);
            }
            else return;
        }
    }
}
