using BrokemiaHelper;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CommunalHelper.Imports;
using Celeste.Mod.GravityHelper.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using FrostHelper;
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
    internal class modSpring : IGameModification
    {
        private static Type DashlessSpringType;
        private static Type CustomSpringType;
        private static Type DashSpringType;
        private static Type VanillaSpringType;
        private static Type MomentumSpringType;
        private static Type GravitySpringType;

        private static Hook hookOnCollideDashless;

        private static Hook NewRenderHook;
        private static Hook NewOnCollideHook;
        private static Hook NewOnHoldableHook;
        private static Hook NewOnPufferHook;

        private static Hook GravityOnCollideHook;
        private static Hook GravityOnPufferHook;
        private static Hook GravityOnHoldableHook;
        private static FieldInfo GravityTypeField;

        private static Hook DashSpringOnCollideHook;

        private delegate void orig_OnCollideOverride(Entity self, Player player);

        public override void Load()
        {
            On.Celeste.Spring.Render += modSpring_Render;
            On.Celeste.Spring.OnCollide += modSpring_OnCollide;
            On.Celeste.Spring.OnHoldable += modSpring_OnHoldable;
            On.Celeste.Spring.OnPuffer += modSpring_OnPuffer;
            On.Celeste.Spring.OnSeeker += modSpring_OnSeeker;

            DashlessSpringType = typeof(NoDashRefillSpring);
            CustomSpringType = typeof(CustomSpring);
            DashSpringType = typeof(DashSpring);
            VanillaSpringType = typeof(Spring);
            MomentumSpringType = typeof(DJMapHelper.Entities.SpringGreen);
            GravitySpringType = typeof(GravitySpring);

            GravityTypeField = GravitySpringType.GetField("GravityType", BindingFlags.Public | BindingFlags.Instance);

            MethodInfo customRender = CustomSpringType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo customCollide = CustomSpringType.GetMethod("NewOnCollide", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo customHoldable = CustomSpringType.GetMethod("NewOnHoldable", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo customPuffer = CustomSpringType.GetMethod("NewOnPuffer", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo dashlessCollideMethod = DashlessSpringType.GetMethod("OnCollideOverride", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo dashCollideMethod = DashSpringType.GetMethod("OnCollide", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo gravityCollideMethod = GravitySpringType.GetMethod("OnCollide", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo gravityPufferMethod = GravitySpringType.GetMethod("OnPuffer", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo gravityHoldableMethod = GravitySpringType.GetMethod("OnHoldable", BindingFlags.NonPublic | BindingFlags.Instance);

            NewRenderHook = new Hook(customRender, typeof(modSpring).GetMethod(nameof(modSpring_Render), BindingFlags.NonPublic | BindingFlags.Static));
            NewOnCollideHook = new Hook(customCollide, typeof(modSpring).GetMethod(nameof(modSpring_OnCollide), BindingFlags.NonPublic | BindingFlags.Static));
            NewOnHoldableHook = new Hook(customHoldable, typeof(modSpring).GetMethod(nameof(modSpring_OnHoldable), BindingFlags.NonPublic | BindingFlags.Static));
            NewOnPufferHook = new Hook(customPuffer, typeof(modSpring).GetMethod(nameof(modSpring_OnPuffer), BindingFlags.NonPublic | BindingFlags.Static));

            hookOnCollideDashless = new Hook(dashlessCollideMethod, typeof(modSpring).GetMethod(nameof(modSpring_OnCollide), BindingFlags.NonPublic | BindingFlags.Static));
            DashSpringOnCollideHook = new Hook(dashCollideMethod, typeof(modSpring).GetMethod(nameof(modSpring_OnCollide), BindingFlags.NonPublic | BindingFlags.Static));

            DashSpringOnCollideHook = new Hook(gravityCollideMethod, typeof(modSpring).GetMethod(nameof(modSpring_OnCollide), BindingFlags.NonPublic | BindingFlags.Static));
            GravityOnPufferHook = new Hook(gravityPufferMethod, typeof(modSpring).GetMethod(nameof(modSpring_OnPuffer), BindingFlags.NonPublic | BindingFlags.Static));
            GravityOnHoldableHook = new Hook(gravityHoldableMethod, typeof(modSpring).GetMethod(nameof(modSpring_OnHoldable), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            On.Celeste.Spring.Render -= modSpring_Render;
            On.Celeste.Spring.OnCollide -= modSpring_OnCollide;
            On.Celeste.Spring.OnHoldable -= modSpring_OnHoldable;
            On.Celeste.Spring.OnPuffer -= modSpring_OnPuffer;
            On.Celeste.Spring.OnSeeker -= modSpring_OnSeeker;

            hookOnCollideDashless?.Dispose();
            hookOnCollideDashless = null;

            NewRenderHook?.Dispose();
            NewOnCollideHook?.Dispose();
            NewOnPufferHook?.Dispose();
            NewOnHoldableHook?.Dispose();
            DashSpringOnCollideHook?.Dispose();

            GravityOnCollideHook?.Dispose();
            GravityOnPufferHook?.Dispose();
            GravityOnHoldableHook?.Dispose();
            GravityOnCollideHook = null;
            GravityOnPufferHook = null;
            GravityOnHoldableHook = null;

            NewRenderHook = null;
            NewOnCollideHook = null;
            NewOnPufferHook = null;
            NewOnHoldableHook = null;
            DashSpringOnCollideHook = null;

        }

        private static void modSpring_Render(On.Celeste.Spring.orig_Render orig, Spring self)
        {
            orig(self);

            if (CelesteArchipelagoModule.shouldModMechanics && !isActive(self))
            {
                self.sprite.Color = Microsoft.Xna.Framework.Color.DarkRed;
            }
        }

        private static void modSpring_OnCollide(On.Celeste.Spring.orig_OnCollide orig, Spring self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self, player);
                return;
            }

            if (isActive(self))
            {
                orig(self, player);
            }
        }

        private static void modSpring_OnHoldable(On.Celeste.Spring.orig_OnHoldable orig, Spring self, Holdable h)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self, h);
                return;
            }

            if (isActive(self))
            {
                orig(self, h);
            }
        }

        private static void modSpring_OnPuffer(On.Celeste.Spring.orig_OnPuffer orig, Spring self, Puffer p)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self, p);
                return;
            }

            if (isActive(self))
            {
                orig(self, p);
            }
        }

        private static void modSpring_OnSeeker(On.Celeste.Spring.orig_OnSeeker orig, Spring self, Seeker seeker)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self, seeker);
                return;
            }

            if (isActive(self))
            {
                orig(self, seeker);
            }
        }

        private static bool isActive(Spring self)
        {
            GravityType gravityType = GravityType.None;
            if(self.GetType() == GravitySpringType)
            {
                gravityType = (GravityType)GravityTypeField.GetValue(self);
            }

            return self.GetType() == VanillaSpringType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS) ||
                self.GetType() == CustomSpringType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS) ||
                self.GetType() == DashlessSpringType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASHLESS_SPRINGS) ||
                self.GetType() == DashSpringType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_SPRING) ||
                self.GetType() == MomentumSpringType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOMENTUM_SPRING) ||
                self.GetType() == GravitySpringType && gravityType == GravityType.Normal && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_GRAVITY_SPRING) ||
                self.GetType() == GravitySpringType && gravityType == GravityType.Inverted && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_GRAVITY_SPRING) ||
                self.GetType() == GravitySpringType && gravityType == GravityType.Toggle && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_GRAVITY_SPRING);
        }
    }
}
