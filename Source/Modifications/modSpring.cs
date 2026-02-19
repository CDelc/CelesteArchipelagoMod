using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modSpring : IGameModification
    {

        static Microsoft.Xna.Framework.Color originalColor;
        static bool setColor = false;

        public override void Load()
        {
            On.Celeste.Spring.Render += modSpring_Render;
            On.Celeste.Spring.OnCollide += modSpring_OnCollide;
            On.Celeste.Spring.OnHoldable += modSpring_OnHoldable;
            On.Celeste.Spring.OnPuffer += modSpring_OnPuffer;
            On.Celeste.Spring.OnSeeker += modSpring_OnSeeker;
        }

        public override void Unload()
        {
            On.Celeste.Spring.Render -= modSpring_Render;
            On.Celeste.Spring.OnCollide -= modSpring_OnCollide;
            On.Celeste.Spring.OnHoldable -= modSpring_OnHoldable;
            On.Celeste.Spring.OnPuffer -= modSpring_OnPuffer;
            On.Celeste.Spring.OnSeeker -= modSpring_OnSeeker;
        }

        private static void modSpring_Render(On.Celeste.Spring.orig_Render orig, Spring self)
        {

            orig(self);

            if (CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                if (!setColor)
                {
                    originalColor = self.sprite.Color;
                    setColor = true;
                }

                if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS))
                {
                    self.sprite.Color = Microsoft.Xna.Framework.Color.DarkRed;
                }
                else
                {
                    self.sprite.Color = originalColor;
                }
            }
        }

        private static void modSpring_OnCollide(On.Celeste.Spring.orig_OnCollide orig, Spring self, Player player)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS))
            {
                orig(self, player);
            }
        }

        private static void modSpring_OnHoldable(On.Celeste.Spring.orig_OnHoldable orig, Spring self, Holdable h)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS))
            {
                orig(self, h);
            }
        }

        private static void modSpring_OnPuffer(On.Celeste.Spring.orig_OnPuffer orig, Spring self, Puffer p)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS))
            {
                orig(self, p);
            }
        }

        private static void modSpring_OnSeeker(On.Celeste.Spring.orig_OnSeeker orig, Spring self, Seeker seeker)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS))
            {
                orig(self, seeker);
            }
        }
    }
}
