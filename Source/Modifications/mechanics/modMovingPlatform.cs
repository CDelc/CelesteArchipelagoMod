using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.MaxHelpingHand.Entities;
using Microsoft.Xna.Framework;
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
    internal class modMovingPlatform : IGameModification
    {

        private static Type MultiNodeMovingPlatformType;

        private static Hook MultiNodeMovingPlatformHook;

        private delegate void orig_Update(MaxHelpingHand.Entities.MultiNodeMovingPlatform self);

        public override void Load()
        {
            MultiNodeMovingPlatformType = CelesteArchipelagoModule.FindType("Celeste.Mod.MaxHelpingHand.Entities.MultiNodeMovingPlatform");

            MethodInfo updateMethod = MultiNodeMovingPlatformType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            MultiNodeMovingPlatformHook = new Hook(updateMethod, typeof(modMovingPlatform).GetMethod(nameof(modMultiNodeMovingPlatform_Update), BindingFlags.NonPublic | BindingFlags.Static));

            On.Celeste.MovingPlatform.Update += modMovingPlatform_Update;
        }

        public override void Unload()
        {
            On.Celeste.MovingPlatform.Update -= modMovingPlatform_Update;
            MultiNodeMovingPlatformHook?.Dispose();
            MultiNodeMovingPlatformHook = null;
        }

        private static void modMovingPlatform_Update(On.Celeste.MovingPlatform.orig_Update orig, MovingPlatform self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
            }
            else if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_PLATFORM))
            {
                self.Collidable = true;
                orig(self);
            }
            else
            {
                Vector2 offset = new Vector2(Monocle.Calc.Random.NextFloat() * 1.5f, Monocle.Calc.Random.NextFloat() * 1.5f);
                self.Position = self.start + offset;
                self.Collidable = false;
                self.Visible = false;
            }
        }

        private static void modMultiNodeMovingPlatform_Update(orig_Update orig, MultiNodeMovingPlatform self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
            }
            else if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_PLATFORM))
            {
                self.Collidable = true;
                orig(self);
            }
            else
            {
                Vector2 offset = new Vector2(Monocle.Calc.Random.NextFloat() * 1.5f, Monocle.Calc.Random.NextFloat() * 1.5f);
                self.Collidable = false;
                self.Visible = false;
            }
        }
    }
}
