using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
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
    internal class modForceJumpCrystal : IGameModification
    {
        private static Type ForceJumpCrystalType;
        private static Hook HookUpdate;
        private delegate void orig_Update(ForceJumpCrystal self);
        private static FieldInfo SpritesField;
        private static FieldInfo FlashSpritesField;
        private static FieldInfo OutlineField;
        private static FieldInfo RespawnTimerField;

        private static BindingFlags privateLookup = BindingFlags.NonPublic | BindingFlags.Instance;
        private static BindingFlags detourLookup = BindingFlags.NonPublic | BindingFlags.Static;
        private static BindingFlags publicLookup = BindingFlags.Public | BindingFlags.Instance;

        public override void Load()
        {
            ForceJumpCrystalType = typeof(ForceJumpCrystal);
            SpritesField = ForceJumpCrystalType.GetField("sprites", privateLookup);
            FlashSpritesField = ForceJumpCrystalType.GetField("flashSprites", privateLookup);
            OutlineField = ForceJumpCrystalType.GetField("outline", privateLookup);
            RespawnTimerField = ForceJumpCrystalType.GetField("respawnTimer", privateLookup);

            HookUpdate = new Hook(ForceJumpCrystalType.GetMethod("Update", publicLookup),
                typeof(modForceJumpCrystal).GetMethod(nameof(modUpdate), detourLookup));
        }

        public override void Unload()
        {
            HookUpdate?.Dispose();
            HookUpdate = null;
        }

        private static void modUpdate(orig_Update orig, ForceJumpCrystal self)
        {
            orig(self);
            if(CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.FORCE_JUMP_CRYSTAL))
            {
                self.Collidable = false;
                List<Sprite> sprites = (List<Sprite> )SpritesField.GetValue(self);
                List<Sprite> flashSprites = (List<Sprite>)FlashSpritesField.GetValue(self);
                Image outline = (Image)OutlineField.GetValue(self);
                foreach(Sprite sprite in sprites)
                {
                    sprite.Visible = false;
                }
                foreach(Sprite flash in flashSprites)
                {
                    flash.Visible = false;
                }
                outline.Visible = true;
                SpritesField.SetValue(self, sprites);
                FlashSpritesField.SetValue(self, flashSprites);
                OutlineField.SetValue(self, outline);
                RespawnTimerField.SetValue(self, 1.0f);
            }
        }
    }
}
