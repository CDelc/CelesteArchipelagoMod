using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modCloud : IGameModification
    {

        static Microsoft.Xna.Framework.Color originalColor;
        static bool setColor = false;

        public override void Load()
        {
            On.Celeste.Cloud.Render += modCloud_Render;
            On.Celeste.Cloud.Update += modCloud_Update;
        }

        public override void Unload()
        {
            On.Celeste.Cloud.Render -= modCloud_Render;
            On.Celeste.Cloud.Update -= modCloud_Update;
        }

        private static void modCloud_Render(On.Celeste.Cloud.orig_Render orig, Cloud self)
        {

            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self);
                return;
            }

            if (!setColor)
            {
                originalColor = self.sprite.Color;
                setColor = true;
            }

            if (isEnabled(self))
            {
                self.sprite.Color = originalColor;
            }
            else
            {
                self.sprite.Color = new Microsoft.Xna.Framework.Color(0.7f, 0.7f, 0.7f, 0.5f);
            }

            orig(self);
        }

        private static void modCloud_Update(On.Celeste.Cloud.orig_Update orig, Cloud self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave) return;

            if (isEnabled(self))
            {
                if (!self.fragile || (self.waiting && self.respawnTimer <= 0.0f))
                {
                    self.Collidable = true;
                }
            }
            else
            {
                self.Collidable = false;
            }
        }

        private static bool isEnabled(Cloud self)
        {
            return self.fragile && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_CLOUDS) ||
                !self.fragile && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CLOUDS);
        }
    }
}
