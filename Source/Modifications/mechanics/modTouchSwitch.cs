using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modTouchSwitch : IGameModification
    {

        static Microsoft.Xna.Framework.Color originalColor;
        static bool setColor = false;
        public override void Load()
        {
            On.Celeste.TouchSwitch.TurnOn += modTurnOn;
            On.Celeste.TouchSwitch.Render += modRender;
        }

        public override void Unload()
        {
            On.Celeste.TouchSwitch.TurnOn -= modTurnOn;
            On.Celeste.TouchSwitch.Render -= modRender;
        }

        private void modRender(On.Celeste.TouchSwitch.orig_Render orig, TouchSwitch self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return;
            }

            if (!setColor)
            {
                originalColor = self.inactiveColor;
                setColor = true;
            }

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TOUCH_SWITCH))
            {
                self.inactiveColor = Color.Red * 0.3f;
            }
            else
            {
                self.inactiveColor = originalColor;
            }
        }

        private static void modTurnOn(On.Celeste.TouchSwitch.orig_TurnOn orig, TouchSwitch self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TOUCH_SWITCH))
            {
                orig(self);
            }
        }
    }
}
