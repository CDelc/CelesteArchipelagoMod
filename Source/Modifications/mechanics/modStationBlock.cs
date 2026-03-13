using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CommunalHelper.Entities;
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
    internal class modStationBlock : IGameModification
    {
        private static Type StationBlockType;

        private static Hook HookOnDashed;
        private static Hook HookRender;

        private delegate DashCollisionResults orig_OnDashed(StationBlock self, Player player, Vector2 direction);
        private delegate void orig_Render(StationBlock self);

        public override void Load()
        {
            StationBlockType = typeof(CommunalHelper.Entities.StationBlock);

            MethodInfo onDashedMethod = StationBlockType.GetMethod("OnDashed", BindingFlags.NonPublic | BindingFlags.Instance, new Type[] { typeof(Player), typeof(Vector2) });
            HookOnDashed = new Hook(onDashedMethod, typeof(modPushBlock).GetMethod(nameof(modOnDashed), BindingFlags.Static | BindingFlags.NonPublic));

            MethodInfo renderMethod = StationBlockType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            HookRender = new Hook(renderMethod, typeof(modPushBlock).GetMethod(nameof(modRender), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            HookOnDashed?.Dispose();
            HookOnDashed = null;

            HookRender?.Dispose();
            HookRender = null;
        }

        private static DashCollisionResults modOnDashed(orig_OnDashed orig, StationBlock self, Player player, Vector2 direction)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, player, direction);
            }
            else if (!isActive(self))
            {
                return DashCollisionResults.Bounce;
            }
            else return orig(self, player, direction);

        }

        private static void modRender(orig_Render orig, StationBlock self)
        {
            orig(self);
            if (CelesteArchipelagoModule.shouldModMechanics && !isActive(self))
            {
                Constants.DrawDisabledRect(self.Collider);
            }
        }

        private static bool isActive(StationBlock self)
        {
            return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PUSH_STATION_BLOCK) && self.ReverseControls ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PULL_STATION_BLOCK) && !self.ReverseControls;
        }
    }
}
