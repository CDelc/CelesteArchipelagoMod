using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modCrumblePlatform : IGameModification
    {
        public override void Load()
        {
            On.Celeste.CrumblePlatform.Added += modAdded;
            On.Celeste.Solid.GetPlayerOnTop += modOnTop;
            On.Celeste.Solid.GetPlayerClimbing += modClimbing;
        }

        public override void Unload()
        {
            On.Celeste.CrumblePlatform.Added -= modAdded;
            On.Celeste.Solid.GetPlayerOnTop -= modOnTop;
            On.Celeste.Solid.GetPlayerClimbing -= modClimbing;
        }

        private Player modClimbing(On.Celeste.Solid.orig_GetPlayerClimbing orig, Solid self)
        {
            if(self is not CrumblePlatform || !CelesteArchipelagoModule.shouldModMechanics ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CRUMBLING_PLATFORM))
            {
                return orig(self);
            }
            else
            {
                return null;
            }
        }

        private Player modOnTop(On.Celeste.Solid.orig_GetPlayerOnTop orig, Solid self)
        {
            if (self is not CrumblePlatform || !CelesteArchipelagoModule.shouldModMechanics ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CRUMBLING_PLATFORM))
            {
                return orig(self);
            }
            else
            {
                return null;
            }
        }

        private static void modAdded(On.Celeste.CrumblePlatform.orig_Added orig, CrumblePlatform self, Scene scene)
        {
            orig(self, scene);
            if(CelesteArchipelagoModule.shouldModMechanics &&
                !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CRUMBLING_PLATFORM))
            {
                self.Collidable = false;
                self.occluder.Visible = false;
                foreach (Image image in self.images)
                {
                    image.Color = Color.Gray * 0.6f;
                }
            }
        }
    }
}
