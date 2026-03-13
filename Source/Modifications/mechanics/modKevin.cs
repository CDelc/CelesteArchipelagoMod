using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CherryHelper;
using Celeste.Mod.MaxHelpingHand.Entities;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modKevin : IGameModification
    {
        private static bool bNeedResetFace = false;

        private static Type ReskinnableCrushBlockType;
        private static Type NonReturnKevinType;

        private static Hook NonReturnKevinRender;
        private static Hook NonReturnKevinCanActivate;

        private delegate void orig_Render(NonReturnCrushBlock self);
        private delegate bool orig_CanActivate(NonReturnCrushBlock self, Vector2 direction);

        public override void Load()
        {
            ReskinnableCrushBlockType = typeof(ReskinnableCrushBlock);
            NonReturnKevinType = typeof(NonReturnCrushBlock);

            MethodInfo NonReturnRenderMethod = NonReturnKevinType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo NonReturnCanActivateMethod = NonReturnKevinType.GetMethod("CanActivate", BindingFlags.Public | BindingFlags.Instance);

            NonReturnKevinRender = new Hook(NonReturnRenderMethod, typeof(modKevin).GetMethod(nameof(nonReturn_Render), BindingFlags.NonPublic | BindingFlags.Static));
            NonReturnKevinCanActivate = new Hook(NonReturnCanActivateMethod, typeof(modKevin).GetMethod(nameof(nonReturn_CanActivate), BindingFlags.NonPublic | BindingFlags.Static));

            On.Celeste.CrushBlock.Render += modCrushBlock_Render;
            On.Celeste.CrushBlock.CanActivate += modCrushBlock_CanActivate;
        }

        public override void Unload()
        {
            On.Celeste.CrushBlock.Render -= modCrushBlock_Render;
            On.Celeste.CrushBlock.CanActivate -= modCrushBlock_CanActivate;

            NonReturnKevinRender?.Dispose();
            NonReturnKevinCanActivate?.Dispose();

            NonReturnKevinRender = null;
            NonReturnKevinCanActivate = null;
        }

        private static void modCrushBlock_Render(On.Celeste.CrushBlock.orig_Render orig, CrushBlock self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            
            if(self.GetType() == ReskinnableCrushBlockType)
            {
                if (SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/2-Intermediate/bryse0n") ||
                    SaveData.Instance.CurrentSession_Safe.Level.Equals("cp2-5-bryse0n"))
                {
                    if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.NEON_BLUE_KEVIN))
                    {
                        self.face.Play("hurt", false, false);
                        bNeedResetFace = true;
                    }

                    if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.NEON_BLUE_KEVIN) && bNeedResetFace)
                    {
                        self.face.Play("idle", false, false);
                        bNeedResetFace = false;
                    }
                }
            }
            else
            {
                if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN))
                {
                    self.face.Play("hurt", false, false);
                    bNeedResetFace = true;
                }

                if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN) && bNeedResetFace)
                {
                    self.face.Play("idle", false, false);
                    bNeedResetFace = false;
                }
            }
            orig(self);
        }

        private static bool modCrushBlock_CanActivate(On.Celeste.CrushBlock.orig_CanActivate orig, CrushBlock self, Vector2 direction)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, direction);
            }
            if (self.GetType() == ReskinnableCrushBlockType && SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/2-Intermediate/bryse0n"))
            {
                if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.NEON_BLUE_KEVIN))
                {
                    return false;
                }
                else return orig(self, direction);
            }
            else
            {
                if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.KEVIN))
                {
                    return false;
                }
                else
                {
                    return orig(self, direction);
                }
            }
        }

        private static void nonReturn_Render(orig_Render orig, NonReturnCrushBlock self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            if (SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/2-Intermediate/bryse0n"))
            {
                if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.NEON_PURPLE_KEVIN))
                {
                    self.face.Play("hurt", false, false);
                    bNeedResetFace = true;
                }

                if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.NEON_PURPLE_KEVIN) && bNeedResetFace)
                {
                    self.face.Play("idle", false, false);
                    bNeedResetFace = false;
                }
            }
            orig(self);
        }

        private static bool nonReturn_CanActivate(orig_CanActivate orig, NonReturnCrushBlock self, Vector2 direction)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, direction);
            }
            if (SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/2-Intermediate/bryse0n"))
            {
                if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.NEON_PURPLE_KEVIN))
                {
                    return false;
                }
                else return orig(self, direction);

            }
            else return orig(self, direction);
        }
    }
}
