using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CommunalHelper.Components;
using Celeste.Mod.CommunalHelper.Entities;
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
using VivHelper.Entities;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modDreamBlock : IGameModification
    {

        private static Type NormalDreamBlockType;
        private static Type CustomDreamBlockType;
        private static Type FrostHelperCustomDreamBlockType;
        private static Type ConnectedDreamBlockType;
        private static Type FallingDreamBlockType;
        private static Type DreamZipMoverType;
        private static Type DreamMoveBlockType;
        private static Type BounceDreamBlockType;
        private static Type DreamSwitchGateType;

        private static FieldInfo RefillCountField;

        private static Hook HookShouldActivate;
        private delegate bool orig_ShouldActivate(DreamZipMover self, out bool isMain);

        private static Hook HookUpdateColors;
        private static Hook HookMoveCheck;
        private delegate void orig_UpdateColors(DreamMoveBlock self);
        private delegate bool orig_MoveCheck(DreamMoveBlock self, Vector2 speed);

        private static Hook HookBounceRender;
        private delegate void orig_BounceRender(DreamBlock self);

        private static Color disabledBackColor = Calc.HexToColor("1f2e2d");
        private static Color disabledLineColor = Calc.HexToColor("6a8480");

        private static MethodInfo WobblelineMethod;

        public override void Load()
        {
            NormalDreamBlockType = CelesteArchipelagoModule.FindType("Celeste.DreamBlock");
            CustomDreamBlockType = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.Entities.CustomDreamBlock");
            ConnectedDreamBlockType = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.Entities.ConnectedDreamBlock");
            FrostHelperCustomDreamBlockType = typeof(FrostHelper.CustomDreamBlockV2);
            FallingDreamBlockType = typeof(DreamFallingBlock);
            DreamZipMoverType = typeof(DreamZipMover);
            DreamMoveBlockType = typeof(DreamMoveBlock);
            BounceDreamBlockType = CelesteArchipelagoModule.FindType("Celeste.Mod.BounceHelper.BounceDreamBlock");
            DreamSwitchGateType = typeof(DreamSwitchGate);

            RefillCountField = CustomDreamBlockType.GetField("RefillCount", BindingFlags.NonPublic | BindingFlags.Instance);

            WobblelineMethod = NormalDreamBlockType.GetMethod("WobbleLine", BindingFlags.NonPublic | BindingFlags.Instance);

            On.Celeste.DreamBlock.Render += modDreamBlock_Render;
            On.Celeste.DreamBlock.Update += modDreamBlock_Update;
            On.Celeste.Player.DreamDashCheck += modPlayer_DreamDashCheck;

            HookShouldActivate = new Hook(DreamZipMoverType.GetMethod("ShouldActivate", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(modDreamBlock).GetMethod(nameof(modShouldActivateTrafficBlock), BindingFlags.Static | BindingFlags.NonPublic));

            HookMoveCheck = new Hook(DreamMoveBlockType.GetMethod("MoveCheck", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(modDreamBlock).GetMethod(nameof(modMoveCheckMoveBlock), BindingFlags.NonPublic | BindingFlags.Static));
            HookUpdateColors = new Hook(DreamMoveBlockType.GetMethod("UpdateColors", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(modDreamBlock).GetMethod(nameof(modUpdateColorsMoveBlock), BindingFlags.NonPublic | BindingFlags.Static));

            HookBounceRender = new Hook(BounceDreamBlockType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance),
                typeof(modDreamBlock).GetMethod(nameof(bounceDreamBlock_Render), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            On.Celeste.DreamBlock.Render -= modDreamBlock_Render;
            On.Celeste.DreamBlock.Update -= modDreamBlock_Update;
            On.Celeste.Player.DreamDashCheck -= modPlayer_DreamDashCheck;

            HookShouldActivate?.Dispose();
            HookShouldActivate = null;

            HookMoveCheck?.Dispose();
            HookUpdateColors?.Dispose();
            HookMoveCheck = null;
            HookUpdateColors = null;
        }

        private static void modDreamBlock_Render(On.Celeste.DreamBlock.orig_Render orig, DreamBlock self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                return;
            }
            else
            {
                self.DisableLightsInside = false;
            }
        }

        private static void bounceDreamBlock_Render(orig_BounceRender orig, DreamBlock self)
        {
            if(CelesteArchipelagoModule.shouldModMechanics && !isActive(self))
            {
                Camera camera = self.SceneAs<Level>().Camera;
                if (self.Right < camera.Left || self.Left > camera.Right || self.Bottom < camera.Top || self.Top > camera.Bottom)
                {
                    return;
                }
                Draw.Rect(self.shake.X + self.X, self.shake.Y + self.Y, self.Width, self.Height, disabledBackColor);
                Vector2 position = self.SceneAs<Level>().Camera.Position;
                for (int i = 0; i < self.particles.Length; i++)
                {
                    int layer = self.particles[i].Layer;
                    Vector2 position2 = self.particles[i].Position;
                    position2 += position * (0.3f + 0.25f * (float)layer);
                    position2 = self.PutInside(position2);
                    Color color = self.particles[i].Color;
                    MTexture mTexture;
                    switch (layer)
                    {
                        case 0:
                            {
                                int num2 = (int)((self.particles[i].TimeOffset * 4f + self.animTimer) % 4f);
                                mTexture = self.particleTextures[3 - num2];
                                break;
                            }
                        case 1:
                            {
                                int num = (int)((self.particles[i].TimeOffset * 2f + self.animTimer) % 2f);
                                mTexture = self.particleTextures[1 + num];
                                break;
                            }
                        default:
                            mTexture = self.particleTextures[2];
                            break;
                    }
                    if (position2.X >= self.X + 2f && position2.Y >= self.Y + 2f && position2.X < self.Right - 2f && position2.Y < self.Bottom - 2f)
                    {
                        mTexture.DrawCentered(position2 + self.shake, color);
                    }
                }
                if (self.whiteFill > 0f)
                {
                    Draw.Rect(self.X + self.shake.X, self.Y + self.shake.Y, self.Width, self.Height * self.whiteHeight, Color.White * self.whiteFill);
                }
                WobbleLine(self, self.shake + new Vector2(self.X, self.Y), self.shake + new Vector2(self.X + self.Width, self.Y), 0f);
                WobbleLine(self, self.shake + new Vector2(self.X + self.Width, self.Y), self.shake + new Vector2(self.X + self.Width, self.Y + self.Height), 0.7f);
                WobbleLine(self, self.shake + new Vector2(self.X + self.Width, self.Y + self.Height), self.shake + new Vector2(self.X, self.Y + self.Height), 1.5f);
                WobbleLine(self, self.shake + new Vector2(self.X, self.Y + self.Height), self.shake + new Vector2(self.X, self.Y), 2.5f);
                Draw.Rect(self.shake + new Vector2(self.X, self.Y), 2f, 2f, disabledLineColor);
                Draw.Rect(self.shake + new Vector2(self.X + self.Width - 2f, self.Y), 2f, 2f, disabledLineColor);
                Draw.Rect(self.shake + new Vector2(self.X, self.Y + self.Height - 2f), 2f, 2f, disabledLineColor);
                Draw.Rect(self.shake + new Vector2(self.X + self.Width - 2f, self.Y + self.Height - 2f), 2f, 2f, disabledLineColor);
            }
            else
            {
                orig(self);
            }
        }

        private static void WobbleLine(DreamBlock self, Vector2 from, Vector2 to, float offset)
        {
            WobblelineMethod.Invoke(self, new object[] {from, to, offset});
        }

        private static void modDreamBlock_Update(On.Celeste.DreamBlock.orig_Update orig, DreamBlock self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                self.playerHasDreamDash = true;
                orig(self);
            }
            else
            {
                self.playerHasDreamDash = false;
            }
        }

        private static bool modPlayer_DreamDashCheck(On.Celeste.Player.orig_DreamDashCheck orig, Player self, Microsoft.Xna.Framework.Vector2 dir)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && self.Inventory.DreamDash && self.DashAttacking && (dir.X == (float)Math.Sign(self.DashDir.X) || dir.Y == (float)Math.Sign(self.DashDir.Y)))
            {
                DreamBlock dreamBlock = self.CollideFirst<DreamBlock>(self.Position + dir);
                if (dreamBlock != null && isActive(dreamBlock))
                {
                    return orig(self, dir);
                }
                else return false;
            }
            return orig(self, dir);
        }

        private static bool modShouldActivateTrafficBlock(orig_ShouldActivate orig, DreamZipMover self, out bool isMain)
        {
            if(!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                return orig(self, out isMain);
            }
            isMain = false;
            return false;
        }

        private static bool modMoveCheckMoveBlock(orig_MoveCheck orig, DreamMoveBlock self, Vector2 speed)
        {
            if (isActive(self) || !CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, speed);
            }
            else
            {
                return true;
            }
        }

        private static void modUpdateColorsMoveBlock(orig_UpdateColors orig, DreamMoveBlock self)
        {
            if (isActive(self) || !CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
            }
            else
            {
                BindingFlags privateLookup = BindingFlags.NonPublic | BindingFlags.Instance;
                BindingFlags publicLookup = BindingFlags.Public | BindingFlags.Instance;
                Color fillColor = MoveBlock.breakingBgFill;
                self.GetType().GetField("fillColor", privateLookup).SetValue(self, fillColor);

                List<Image> topButton = (List<Image>)self.GetType().GetField("topButton", privateLookup).GetValue(self);
                List<Image> leftButton = (List<Image>)self.GetType().GetField("leftButton", privateLookup).GetValue(self);
                List<Image> rightButton = (List<Image>)self.GetType().GetField("rightButton", privateLookup).GetValue(self);

                foreach (Image image in topButton)
                {
                    image.Color = fillColor;
                }
                foreach (Image image2 in leftButton)
                {
                    image2.Color = fillColor;
                }
                foreach (Image image3 in rightButton)
                {
                    image3.Color = fillColor;
                }

                self.GetType().GetField("topButton", privateLookup).SetValue(self, topButton);
                self.GetType().GetField("leftButton", privateLookup).SetValue(self, leftButton);
                self.GetType().GetField("rightButton", privateLookup).SetValue(self, rightButton);

                FieldInfo GroupableField = DreamMoveBlockType.GetField("groupable", privateLookup);
                ((GroupableMoveBlock)GroupableField.GetValue(self)).State = GroupableMoveBlock.MovementState.Idling;
                //groupable.State = GroupableMoveBlock.MovementState.Idling;
                //GroupableField.SetValue(self, groupable);
            }
        }

        private static bool isActive(DreamBlock self)
        {
            int refillCount = 0;
            Color FrostActiveBackColor = new Color();
            if(self.GetType() == ConnectedDreamBlockType)
            {
                refillCount = (int)RefillCountField.GetValue(self);
            }
            if(self.GetType() == FrostHelperCustomDreamBlockType)
            {
                FrostActiveBackColor = ((CustomDreamBlockV2)self).ActiveBackColor;
            }

            return self.GetType() == NormalDreamBlockType && !isSummitDownSide() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK) ||
                self.GetType() == NormalDreamBlockType && isSummitDownSide() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.WHITE_DREAM_BLOCK) ||
                self.GetType() == FrostHelperCustomDreamBlockType && isHoneyDreamColor(FrostActiveBackColor) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BOUNCE_DREAM_BLOCKS) ||
                self.GetType() == FrostHelperCustomDreamBlockType && isWhite(FrostActiveBackColor) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.WHITE_DREAM_BLOCK) ||
                self.GetType() == FrostHelperCustomDreamBlockType && !isHoneyDreamColor(FrostActiveBackColor) && !isWhite(FrostActiveBackColor) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK) ||
                self.GetType() == ConnectedDreamBlockType && refillCount == 2 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_DREAM_BLOCK) ||
                self.GetType() == ConnectedDreamBlockType && refillCount != 2 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK) ||
                self.GetType() == DreamSwitchGateType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK) ||
                self.GetType() == FallingDreamBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK) ||
                self.GetType() == BounceDreamBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK) ||
                self.GetType() == DreamZipMoverType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_TRAFFIC_BLOCK) ||
                self.GetType() == DreamMoveBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_MOVE_BLOCK);
        }

        private static bool isHoneyDreamColor(Color color)
        {
            return color.R == 82 && color.G == 40 && color.B == 0;
        }

        private static bool isWhite(Color color)
        {
            return color.R == 255 && color.G == 255 && color.B == 255;
        }

        private static bool isSummitDownSide()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/4-Expert/Linj") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("e01_linj");
        }
    }
}
