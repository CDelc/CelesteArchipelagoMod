using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.TwigHelper.Entities;
using ExtendedVariants.Entities.ForMappers;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using vitmod;
using VivHelper.Entities;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modShroomRefill : IGameModification
    {
        private static Type ShroomRefillType;
        private static Type WrapperRefillWallType;
        private static Hook HookUpdate;
        private static Hook HookRenderWall;
        private delegate void orig_Update(ShroomRefill self);
        private delegate void orig_Render(WrapperRefillWall self);
        private static FieldInfo SpriteField;
        private static FieldInfo OutlineField;
        private static FieldInfo RespawnTimerField;
        private static FieldInfo RespawnTimerFieldWall;
        private static FieldInfo TiedEntityField;

        private static BindingFlags privateLookup = BindingFlags.NonPublic | BindingFlags.Instance;
        private static BindingFlags detourLookup = BindingFlags.NonPublic | BindingFlags.Static;
        private static BindingFlags publicLookup = BindingFlags.Public | BindingFlags.Instance;

        public override void Load()
        {
            ShroomRefillType = typeof(ShroomRefill);
            WrapperRefillWallType = typeof(WrapperRefillWall);
            SpriteField = ShroomRefillType.GetField("sprite", privateLookup);
            OutlineField = ShroomRefillType.GetField("outline", privateLookup);
            RespawnTimerField = ShroomRefillType.GetField("respawnTimer", privateLookup);
            RespawnTimerFieldWall = WrapperRefillWallType.GetField("respawnTimer", privateLookup);
            TiedEntityField = WrapperRefillWallType.GetField("tiedEntity", privateLookup);

            HookUpdate = new Hook(ShroomRefillType.GetMethod("Update", publicLookup),
                typeof(modShroomRefill).GetMethod(nameof(modUpdate), detourLookup));
            HookRenderWall = new Hook(WrapperRefillWallType.GetMethod("Render", publicLookup),
                typeof(modShroomRefill).GetMethod(nameof(modRenderWall), detourLookup));
        }

        public override void Unload()
        {
            HookUpdate?.Dispose();
            HookUpdate = null;

            HookRenderWall?.Dispose();
            HookRenderWall = null;
        }

        private static void modUpdate(orig_Update orig, ShroomRefill self)
        {
            orig(self);
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPEED_MUSHROOMS))
            {
                self.Collidable = false;
                Sprite sprite = (Sprite)SpriteField.GetValue(self);
                Image outline = (Image)OutlineField.GetValue(self);
                outline.Visible = true;
                sprite.Visible = false;
                SpriteField.SetValue(self, sprite);
                OutlineField.SetValue(self, outline);
                RespawnTimerField.SetValue(self, 1.0f);
            }
        }

        private static void modRenderWall(orig_Render orig, WrapperRefillWall self)
        {
            Entity tiedEntity = (Entity)TiedEntityField.GetValue(self);
            if (tiedEntity.GetType() == ShroomRefillType && CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPEED_MUSHROOM_WALL))
            {
                RespawnTimerFieldWall.SetValue(self, 1.0f);
                self.Collidable = false;
            }
            //Jump refill walls here just because
            else if(tiedEntity.GetType() == typeof(JumpRefill) && CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.JUMP_REFILL_WALL))
            {
                RespawnTimerFieldWall.SetValue(self, 1.0f);
                self.Collidable = false;
            }
            else self.Collidable = true;
            orig(self);
        }
    }
}
