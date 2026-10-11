using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.MaxHelpingHand.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modCrumblePlatform : IGameModification
    {

        Hook CustomizableCollisionHook;
        public override void Load()
        {
            On.Celeste.CrumblePlatform.Added += modAdded;
            On.Celeste.Solid.GetPlayerOnTop += modOnTop;
            On.Celeste.Solid.GetPlayerClimbing += modClimbing;
            On.Celeste.Solid.Update += modUpdate;

            CustomizableCollisionHook = new Hook(typeof(CustomizableCrumblePlatform).GetMethod("Added", BindingFlags.Public | BindingFlags.Instance),
                typeof(modCrumblePlatform).GetMethod(nameof(modAdded), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            On.Celeste.CrumblePlatform.Added -= modAdded;
            On.Celeste.Solid.GetPlayerOnTop -= modOnTop;
            On.Celeste.Solid.GetPlayerClimbing -= modClimbing;
            On.Celeste.Solid.Update -= modUpdate;
            CustomizableCollisionHook?.Dispose();
            CustomizableCollisionHook = null;
        }

        private void modUpdate(On.Celeste.Solid.orig_Update orig, Solid self)
        {
            if ((self is not CrumblePlatform && self is not CustomizableCrumblePlatform) || !CelesteArchipelagoModule.shouldModMechanics ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CRUMBLING_PLATFORM))
            {
                orig(self);
            }
            else
            {
                orig(self);
                Player player = self.Scene.Tracker.GetEntity<Player>();
                self.Collidable = player != null && player.Top >= self.Bottom - 1f;
            }
        }

        private static Player modClimbing(On.Celeste.Solid.orig_GetPlayerClimbing orig, Solid self)
        {
            if((self is not CrumblePlatform && self is not CustomizableCrumblePlatform) || !CelesteArchipelagoModule.shouldModMechanics ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CRUMBLING_PLATFORM))
            {
                return orig(self);
            }
            else
            {
                return null;
            }
        }

        private static Player modOnTop(On.Celeste.Solid.orig_GetPlayerOnTop orig, Solid self)
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
                foreach (Image image in self.images)
                {
                    image.Color *= 0.3f;
                }
            }
        }
    }
}
