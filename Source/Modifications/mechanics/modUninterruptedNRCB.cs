using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CherryHelper;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modUninterruptedNRCB : IGameModification
    {

        private static Hook hookRender;
        private static Hook hookCanActivate;

        private static Type BlueKevinType;

        private delegate void orig_Render(UninterruptedNRCB self);
        private delegate bool orig_CanActivate(UninterruptedNRCB self, Vector2 direction);

        private static bool bNeedResetFace = false;

        public override void Load()
        {
            BlueKevinType = typeof(UninterruptedNRCB);

            MethodInfo renderMethod = BlueKevinType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo canActiveMethod = BlueKevinType.GetMethod("CanActivate", BindingFlags.Public | BindingFlags.Instance);

            hookRender = new Hook(renderMethod, typeof(modUninterruptedNRCB).GetMethod(nameof(modRender), BindingFlags.NonPublic | BindingFlags.Static));
            hookCanActivate = new Hook(canActiveMethod, typeof(modUninterruptedNRCB).GetMethod(nameof(modCanActivate), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            hookRender?.Dispose();
            hookCanActivate?.Dispose();

            hookRender = null;
            hookCanActivate = null;
        }

        private static void modRender(orig_Render orig, UninterruptedNRCB self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PUZZLE_KEVIN))
            {
                self.face.Play("hurt", false, false);
                bNeedResetFace = true;
            }

            if (CelesteArchipelagoModule.shouldModMechanics && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PUZZLE_KEVIN) && bNeedResetFace)
            {
                self.face.Play("idle", false, false);
                bNeedResetFace = false;
            }

            orig(self);
        }

        private static bool modCanActivate(orig_CanActivate orig, UninterruptedNRCB self, Vector2 direction)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PUZZLE_KEVIN))
            {
                return false;
            }
            else
            {
                return orig(self, direction);
            }
        }
    }
}
