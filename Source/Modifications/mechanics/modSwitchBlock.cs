using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.VortexHelper.Entities;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modSwitchBlock : IGameModification
    {
        
        private static Hook hookIsActive;

        //ColorSwitch
        private static Hook hookDashed;
        private static Hook hookRender;

        private delegate DashCollisionResults orig_Dashed(ColorSwitch self, Player player, Vector2 direction);
        private delegate bool orig_isActive(VortexHelper.VortexHelperSession.SwitchBlockColor self);
        private delegate void orig_Render(ColorSwitch self);

        private static Type EnumExtType;
        private static Type ColorSwitchType;

        private static FieldInfo CurrentBackgroundColorField;

        public override void Load()
        {
            EnumExtType = CelesteArchipelagoModule.FindType("Celeste.Mod.VortexHelper.Misc.Extensions.EnumExt");
            ColorSwitchType = CelesteArchipelagoModule.FindType("Celeste.Mod.VortexHelper.Entities.ColorSwitch");

            CurrentBackgroundColorField = ColorSwitchType.GetField("currentBackgroundColor", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo isActiveMethod = EnumExtType.GetMethod("IsActive", BindingFlags.Public | BindingFlags.Static);
            hookIsActive = new Hook(isActiveMethod, typeof(modSwitchBlock).GetMethod(nameof(modIsActive), BindingFlags.NonPublic | BindingFlags.Static));

            MethodInfo dashedMethod = ColorSwitchType.GetMethod("Dashed", BindingFlags.NonPublic | BindingFlags.Instance);
            hookDashed = new Hook(dashedMethod, typeof(modSwitchBlock).GetMethod(nameof(modDashed), BindingFlags.NonPublic | BindingFlags.Static));

            MethodInfo renderMethod = ColorSwitchType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            hookRender = new Hook(renderMethod, typeof(modSwitchBlock).GetMethod(nameof(modRender), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            hookIsActive?.Dispose();
            hookIsActive = null;

            hookDashed?.Dispose();
            hookDashed = null;

            hookRender?.Dispose();
            hookRender = null;
        }

        private static bool modIsActive(orig_isActive orig, VortexHelper.VortexHelperSession.SwitchBlockColor self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self);
            }
            else if (self == VortexHelper.VortexHelperSession.SwitchBlockColor.Lime && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_SWITCH_BLOCK))
            {
                return false;
            }
            else if (self == VortexHelper.VortexHelperSession.SwitchBlockColor.Orange && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.ORANGE_SWITCH_BLOCK))
            {
                return false;
            }
            else return orig(self);
        }

        private static DashCollisionResults modDashed(orig_Dashed orig, ColorSwitch self, Player player, Vector2 dir)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SWITCH_BLOCK_SWITCH))
            {
                return orig(self, player, dir);
            }
            else return DashCollisionResults.NormalCollision;
        }

        private static void modRender(orig_Render orig, ColorSwitch self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SWITCH_BLOCK_SWITCH))
            {
                CurrentBackgroundColorField.SetValue(self, new Color(100, 0, 0));
            }
            orig(self);
        }
    }
}
