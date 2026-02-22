using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modBubbles : IGameModification
    {

        public override void Load()
        {
            On.Celeste.Booster.Render += modBooster_Render;
            On.Celeste.Booster.Update += modBooster_Update;
        }

        public override void Unload()
        {
            On.Celeste.Booster.Render -= modBooster_Render;
            On.Celeste.Booster.Update -= modBooster_Update;
        }

        private static void modBooster_Render(On.Celeste.Booster.orig_Render orig, Booster self)
        {
            if (!isEnabled(self))
            {
                self.sprite.Visible = false;
                self.outline.Visible = true;
                self.respawnTimer = 1.0f;
            }

            orig(self);
        }

        private static void modBooster_Update(On.Celeste.Booster.orig_Update orig, Booster self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave) return;

            //self.Ch9HubBooster = false;
            //self.Ch9HubTransition = false;

            if (isEnabled(self))
            {
                self.Collidable = true;
            }
            else
            {
                self.Collidable = false;
            }
        }

        private static bool isEnabled(Booster self)
        {
            return self.red && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_BUBBLES) ||
                !self.red && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_BUBBLES);
        }

    }
}
