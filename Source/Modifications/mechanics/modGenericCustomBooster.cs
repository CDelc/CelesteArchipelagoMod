using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using FrostHelper.Entities.Boosters;
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
    internal class modGenericCustomBooster : IGameModification
    {

        private static Type GenericCustomBoosterType;
        private static Type BlueBoosterType;
        private static Type GrayBoosterType;

        private static Hook hookRender;
        private static Hook hookUpdate;

        private delegate void orig_Render(Entity self);
        private delegate void orig_Update(Entity self);

        public override void Load()
        {
            GenericCustomBoosterType = CelesteArchipelagoModule.FindType("FrostHelper.Entities.Boosters.GenericCustomBooster");
            BlueBoosterType = CelesteArchipelagoModule.FindType("FrostHelper.Entities.Boosters.BlueBooster");
            GrayBoosterType = typeof(GrayBooster);

            MethodInfo renderMethod = GenericCustomBoosterType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            hookRender = new Hook(renderMethod, typeof(modGenericCustomBooster).GetMethod(nameof(modRender), BindingFlags.NonPublic | BindingFlags.Static));

            MethodInfo updateMethod = GenericCustomBoosterType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            hookUpdate = new Hook(updateMethod, typeof(modGenericCustomBooster).GetMethod(nameof(modUpdate), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            hookRender?.Dispose();
            hookUpdate?.Dispose();
            hookRender = null;
            hookUpdate = null;
        }

        private static void modRender(orig_Render orig, Entity self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }

            GenericCustomBooster booster = (GenericCustomBooster)self;

            if (!isActive(self))
            {
                booster.sprite.Visible = false;
                booster.outline.Visible = true;
                booster.respawnTimer = 1.0f;
            }
            orig(self);
        }

        private static void modUpdate(orig_Render orig, Entity self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;
            if (isActive(self))
            {
                self.Collidable = true;
            }
            else
            {
                self.Collidable = false;
            }

        }

        private static bool isActive(Entity self)
        {
            GenericCustomBooster booster = (GenericCustomBooster)self;
            return self.GetType() == BlueBoosterType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SOAP_BUBBLE) && !booster.Red ||
                self.GetType() == BlueBoosterType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_DASHLESS_BUBBLE) && booster.Red ||
                self.GetType() == GrayBoosterType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GRAY_BUBBLES) && !booster.Red;
        }
    }
}
