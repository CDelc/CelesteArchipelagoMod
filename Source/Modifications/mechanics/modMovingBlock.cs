using Celeste.Mod.BounceHelper;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CommunalHelper.Components;
using Celeste.Mod.CommunalHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using vitmod;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modMovingBlock : IGameModification
    {

        private static Type VitMoveBlockType;
        private static Type VanillaMoveBlockType;
        private static Type ConnectedMoveBlockType;
        private static Type BounceMoveBlockType;

        private static Hook VanillaMoveBlockUpdateColors;
        private static Hook VanillaMoveBlockMoveCheck;

        private static Hook VitMoveBlockUpdateColors;
        private static Hook VitMoveBlockMoveCheck;

        private static Hook ConnectedMoveBlockUpdateColors;
        private static Hook ConnectedMoveBlockMoveCheck;

        private static Hook BounceMoveBlockUpdateColors;
        private static Hook BounceMoveBlockMoveCheckH;
        private static Hook BounceMoveBlockMoveCheckV;

        private static FieldInfo ConnectedMoveBlockGroupableField;

        private delegate void orig_OnStaticMoverTrigger(Solid self, StaticMover sm);
        private delegate void orig_UpdateColors(Solid self);
        private delegate bool orig_MoveCheck(Solid self, Vector2 speed);

        private delegate bool orig_MoveCheckBounce(Solid self, float move);

        public override void Load()
        {
            VitMoveBlockType = typeof(VitMoveBlock);
            VanillaMoveBlockType = typeof(MoveBlock);
            ConnectedMoveBlockType = typeof(ConnectedMoveBlock);
            BounceMoveBlockType = typeof(BounceMoveBlock);

            ConnectedMoveBlockGroupableField = ConnectedMoveBlockType.GetField("groupable", BindingFlags.Instance | BindingFlags.NonPublic);

            MethodInfo UpdateColorsMethod = typeof(modMovingBlock).GetMethod(nameof(modMoveBlock_UpdateColors), BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo MoveCheckMethod = typeof(modMovingBlock).GetMethod(nameof(modMoveBlock_MoveCheck), BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo BounceMoveCheckMethod = typeof(modMovingBlock).GetMethod(nameof(modMoveBlock_MoveCheckBounce), BindingFlags.NonPublic | BindingFlags.Static);

            BindingFlags MethodLookupBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            VanillaMoveBlockUpdateColors = new Hook(VanillaMoveBlockType.GetMethod("UpdateColors", MethodLookupBindingFlags), UpdateColorsMethod);
            VanillaMoveBlockMoveCheck = new Hook(VanillaMoveBlockType.GetMethod("MoveCheck", MethodLookupBindingFlags), MoveCheckMethod);
            VitMoveBlockUpdateColors = new Hook(VitMoveBlockType.GetMethod("UpdateColors", MethodLookupBindingFlags), UpdateColorsMethod);
            VitMoveBlockMoveCheck = new Hook(VitMoveBlockType.GetMethod("MoveCheck", MethodLookupBindingFlags), MoveCheckMethod);
            ConnectedMoveBlockUpdateColors = new Hook(ConnectedMoveBlockType.GetMethod("UpdateColors", MethodLookupBindingFlags), UpdateColorsMethod);
            ConnectedMoveBlockMoveCheck = new Hook(ConnectedMoveBlockType.GetMethod("MoveCheck", MethodLookupBindingFlags), MoveCheckMethod);
            BounceMoveBlockUpdateColors = new Hook(BounceMoveBlockType.GetMethod("UpdateColors", MethodLookupBindingFlags), UpdateColorsMethod);
            BounceMoveBlockMoveCheckH = new Hook(BounceMoveBlockType.GetMethod("MoveHCheck", MethodLookupBindingFlags), BounceMoveCheckMethod);
            BounceMoveBlockMoveCheckV = new Hook(BounceMoveBlockType.GetMethod("MoveVCheck", MethodLookupBindingFlags), BounceMoveCheckMethod);
        }

        public override void Unload()
        {
            VanillaMoveBlockUpdateColors?.Dispose();
            VanillaMoveBlockMoveCheck?.Dispose();
            VitMoveBlockUpdateColors?.Dispose();
            VitMoveBlockMoveCheck?.Dispose();
            ConnectedMoveBlockUpdateColors?.Dispose();
            ConnectedMoveBlockMoveCheck?.Dispose();
            BounceMoveBlockUpdateColors?.Dispose();
            BounceMoveBlockMoveCheckH?.Dispose();
            BounceMoveBlockMoveCheckV?.Dispose();

            VanillaMoveBlockUpdateColors = null;
            VanillaMoveBlockMoveCheck = null;
            VitMoveBlockUpdateColors = null;
            VitMoveBlockMoveCheck = null;
            ConnectedMoveBlockUpdateColors = null;
            ConnectedMoveBlockMoveCheck = null;
            BounceMoveBlockUpdateColors = null;
            BounceMoveBlockMoveCheckH = null;
            BounceMoveBlockMoveCheckV = null;
        }

        private static void modMoveBlock_UpdateColors(orig_UpdateColors orig, Solid self)
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

                if(self.GetType() != ConnectedMoveBlockType && self.GetType() != BounceMoveBlockType)
                {
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
                }
                if(self.GetType() == ConnectedMoveBlockType)
                {
                    GroupableMoveBlock groupable = (GroupableMoveBlock)ConnectedMoveBlockGroupableField.GetValue(self);
                    groupable.State = GroupableMoveBlock.MovementState.Idling;
                    ConnectedMoveBlockGroupableField.SetValue(self, groupable);
                }
                else
                {
                    self.GetType().GetField("state", self.GetType() == VanillaMoveBlockType || self.GetType() == BounceMoveBlockType ? privateLookup : publicLookup)
                        .SetValue(self, 0);
                }
            }
        }

        private static bool modMoveBlock_MoveCheck(orig_MoveCheck orig, Solid self, Vector2 speed)
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

        private static bool modMoveBlock_MoveCheckBounce(orig_MoveCheckBounce orig, Solid self, float move)
        {
            if (isActive(self) || !CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, move);
            }
            else
            {
                return true;
            }
        }

        private static bool isActive(Solid self)
        {
            return self.GetType() == VanillaMoveBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_BLOCK) ||
                self.GetType() == VitMoveBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_BLOCK) ||
                self.GetType() == ConnectedMoveBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_BLOCK) ||
                self.GetType() == BounceMoveBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_BLOCK);
        }

    }
}
