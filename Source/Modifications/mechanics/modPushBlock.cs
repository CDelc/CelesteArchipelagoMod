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
    internal class modPushBlock : IGameModification
    {

        private static Type PushBlockType;

        private static Hook HookOnDashed;
        private static Hook HookRender;

        private delegate DashCollisionResults orig_OnDashed(Solid self, Player player, Vector2 direction);
        private delegate void orig_Render(Solid self);

        public override void Load()
        {
            PushBlockType = CelesteArchipelagoModule.FindType("Celeste.Mod.CanyonHelper.PushBlock");
            
            MethodInfo onDashedMethod = PushBlockType.GetMethod("OnDashed", BindingFlags.NonPublic | BindingFlags.Instance, new Type[] {typeof(Player), typeof(Vector2)});
            HookOnDashed = new Hook(onDashedMethod, typeof(modPushBlock).GetMethod(nameof(modOnDashed), BindingFlags.Static | BindingFlags.NonPublic));

            MethodInfo renderMethod = PushBlockType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            HookRender = new Hook(renderMethod, typeof(modPushBlock).GetMethod(nameof(modRender), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            HookOnDashed?.Dispose();
            HookOnDashed = null;

            HookRender?.Dispose();
            HookRender = null;
        }

        private static DashCollisionResults modOnDashed(orig_OnDashed orig, Solid self, Player player, Vector2 direction)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, player, direction);
            }
            else if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PUSH_BLOCK))
            {
                return DashCollisionResults.NormalCollision;
            }
            else return orig(self, player, direction);
            
        }

        private static void modRender(orig_Render orig, Solid self)
        {
            orig(self);
            if(CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PUSH_BLOCK))
            {
                Constants.DrawDisabledRect(self.Collider);
            }
        }
    }
}
