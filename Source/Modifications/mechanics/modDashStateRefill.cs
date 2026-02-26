using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
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
    internal class modDashStateRefill : IGameModification
    {
        private static Type DashStateRefillType;
        private static Type DreamTunnelRefillType;

        private static Hook hookOnPlayer;

        private delegate void orig_OnPlayer(Entity self, Player player);

        public override void Load()
        {
            DashStateRefillType = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.DashStates.DashStateRefill");
            DreamTunnelRefillType = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.DashStates.DreamTunnelRefill");

            MethodInfo onPlayerMethod = DashStateRefillType.GetMethod("OnPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            hookOnPlayer = new Hook(onPlayerMethod, typeof(modDashStateRefill).GetMethod(nameof(modOnPlayer), BindingFlags.NonPublic | BindingFlags.Static));

        }

        public override void Unload()
        {
            hookOnPlayer?.Dispose();
            hookOnPlayer = null;
        }

        private static void modOnPlayer(orig_OnPlayer orig, Entity self, Player player)
        {
            if (self.GetType() != DreamTunnelRefillType ||
                !CelesteArchipelagoModule.shouldModMechanics ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_DASH_CRYSTALS))
            {
                orig(self, player);
            }
        }
    }
}
