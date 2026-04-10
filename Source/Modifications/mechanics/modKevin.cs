using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CherryHelper;
using Celeste.Mod.CommunalHelper;
using Celeste.Mod.MaxHelpingHand.Entities;
using FrostHelper;
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
    internal class modKevin : IGameModification
    {
        private static bool bNeedResetFace = false;

        private static Type ReskinnableCrushBlockType;
        private static Type NonReturnKevinType;
        private static Type TimeKevinType;
        private static Type CustomCrushBlockType;

        private static FieldInfo CustomFaceField;

        private static Hook NonReturnKevinRender;
        private static Hook NonReturnKevinCanActivate;

        private static Hook CustomCrushBlockRender;
        private static Hook CustomCrushBlockCanActivate;

        private delegate void orig_Render(NonReturnCrushBlock self);
        private delegate bool orig_CanActivate(NonReturnCrushBlock self, Vector2 direction);

        private delegate void orig_CustomRender(CustomCrushBlock self);
        private delegate bool orig_CustomCanActivate(CustomCrushBlock self, Vector2 direction);

        public override void Load()
        {
            ReskinnableCrushBlockType = typeof(ReskinnableCrushBlock);
            NonReturnKevinType = typeof(NonReturnCrushBlock);
            TimeKevinType = CelesteArchipelagoModule.FindType("Celeste.Mod.Spirialis.TimeKevin");
            CustomCrushBlockType = typeof(CustomCrushBlock);

            CustomFaceField = CustomCrushBlockType.GetField("_face", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo NonReturnRenderMethod = NonReturnKevinType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo NonReturnCanActivateMethod = NonReturnKevinType.GetMethod("CanActivate", BindingFlags.Public | BindingFlags.Instance);

            MethodInfo CustomRenderMethod = CustomCrushBlockType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo CustomCanActivateMethod = CustomCrushBlockType.GetMethod("CanActivate", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Vector2) }, null);

            NonReturnKevinRender = new Hook(NonReturnRenderMethod, typeof(modKevin).GetMethod(nameof(nonReturn_Render), BindingFlags.NonPublic | BindingFlags.Static));
            NonReturnKevinCanActivate = new Hook(NonReturnCanActivateMethod, typeof(modKevin).GetMethod(nameof(nonReturn_CanActivate), BindingFlags.NonPublic | BindingFlags.Static));

            CustomCrushBlockRender = new Hook(CustomRenderMethod, typeof(modKevin).GetMethod(nameof(custom_Render), BindingFlags.NonPublic | BindingFlags.Static));
            CustomCrushBlockCanActivate = new Hook(CustomCanActivateMethod, typeof(modKevin).GetMethod(nameof(custom_CanActivate), BindingFlags.NonPublic | BindingFlags.Static));

            On.Celeste.CrushBlock.Render += modCrushBlock_Render;
            On.Celeste.CrushBlock.CanActivate += modCrushBlock_CanActivate;
        }

        public override void Unload()
        {
            On.Celeste.CrushBlock.Render -= modCrushBlock_Render;
            On.Celeste.CrushBlock.CanActivate -= modCrushBlock_CanActivate;

            NonReturnKevinRender?.Dispose();
            NonReturnKevinCanActivate?.Dispose();
            CustomCrushBlockRender?.Dispose();
            CustomCrushBlockCanActivate?.Dispose();

            NonReturnKevinRender = null;
            NonReturnKevinCanActivate = null;
            CustomCrushBlockRender = null;
            CustomCrushBlockCanActivate = null;
        }

        private static void modCrushBlock_Render(On.Celeste.CrushBlock.orig_Render orig, CrushBlock self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }

            if (!IsActive(self))
            {
                self.face.Play("hurt", false, false);
                bNeedResetFace = true;
            }

            if (IsActive(self) && bNeedResetFace)
            {
                self.face.Play("idle", false, false);
                bNeedResetFace = false;
            }
            orig(self);
        }

        private static bool modCrushBlock_CanActivate(On.Celeste.CrushBlock.orig_CanActivate orig, CrushBlock self, Vector2 direction)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, direction);
            }
            if (!IsActive(self))
            {
                return false;
            }
            else return orig(self, direction);
        }

        private static void nonReturn_Render(orig_Render orig, NonReturnCrushBlock self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }

            if (!IsActive(self))
            {
                self.face.Play("hurt", false, false);
                bNeedResetFace = true;
            }

            if (IsActive(self) && bNeedResetFace)
            {
                self.face.Play("idle", false, false);
                bNeedResetFace = false;
            }
            orig(self);
        }

        private static bool nonReturn_CanActivate(orig_CanActivate orig, NonReturnCrushBlock self, Vector2 direction)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, direction);
            }

            if (!IsActive(self))
            {
                return false;
            }
            else return orig(self, direction);
        }

        private static void custom_Render(orig_CustomRender orig, CustomCrushBlock self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN))
            {
                ((Sprite)CustomFaceField.GetValue(self)).Play("hurt", false, false);
                bNeedResetFace = true;
            }

            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN) && bNeedResetFace)
            {
                ((Sprite)CustomFaceField.GetValue(self)).Play("idle", false, false);
                bNeedResetFace = false;
            }
            orig(self);
        }

        private static bool custom_CanActivate(orig_CustomCanActivate orig, CustomCrushBlock self, Vector2 direction)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, direction);
            }

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN))
            {
                return false;
            }
            else return orig(self, direction);
        }

        private static bool IsActive(NonReturnCrushBlock self)
        {
            return isNeonLevel() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.NEON_PURPLE_KEVIN) ||
                !isNeonLevel() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN);
        }

        private static bool IsActive(CrushBlock self)
        {
            return self.GetType() == ReskinnableCrushBlockType && isNeonLevel() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.NEON_BLUE_KEVIN) ||
                self.GetType() == typeof(CrushBlock) && !isNeonLevel() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN) ||
                self.GetType() == TimeKevinType && !isNeonLevel() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN);
        }

        private static bool isNeonLevel()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/2-Intermediate/bryse0n") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("cp2-5-bryse0n");

        }
    }
}
