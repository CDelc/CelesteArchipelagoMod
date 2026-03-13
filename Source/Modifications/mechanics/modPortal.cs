using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.OutbackHelper;
using FrostHelper;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modPortal : IGameModification
    {

        private static Type PortalType;

        private static Hook onPlayerHook;
        private static Hook onUpdateHook;

        private static FieldInfo readyColorField;

        private delegate void orig_OnPlayer(Portal self, Player player);
        private delegate void orig_Update(Portal self);

        private static HashSet<int> mappedColors = new HashSet<int> {0, 1, 2, 3, 4};

        public override void Load()
        {
            PortalType = typeof(OutbackHelper.Portal);
            readyColorField = PortalType.GetField("readyColor", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo onPlayerMethod = PortalType.GetMethod("OnPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo updateMethod = PortalType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);

            onPlayerHook = new Hook(onPlayerMethod, typeof(modPortal).GetMethod(nameof(modOnPlayer), BindingFlags.Static | BindingFlags.NonPublic));
            onUpdateHook = new Hook(updateMethod, typeof(modPortal).GetMethod(nameof(modUpdate), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            onPlayerHook?.Dispose();
            onUpdateHook?.Dispose();

            onPlayerHook = null;
            onUpdateHook = null;
        }

        private static void modOnPlayer(orig_OnPlayer orig, Portal self, Player player)
        {
            int readyColor = (int)readyColorField.GetValue(self);
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self, player);
                return;
            }

            if (!isActive(self))
            {
                return;
            }
            else
            {
                orig(self, player);
            }
        }

        private static void modUpdate(orig_Update orig, Portal self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }

            int readyColor = (int)readyColorField.GetValue(self);

            if (!isActive(self))
            {
                self.portal.Color = new Microsoft.Xna.Framework.Color(1, 1, 1, 0.1f);
            }
            orig(self);
        }

        private static bool isActive(Portal self)
        {
            int readyColor = (int)readyColorField.GetValue(self);
            return readyColor == 3 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_PORTAL) ||
                readyColor == 0 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_PORTAL) ||
                readyColor == 1 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_PORTAL) ||
                readyColor == 2 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_PORTAL) ||
                readyColor == 4 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_PORTAL) ||
                !mappedColors.Contains(readyColor);

        }
    }
}
