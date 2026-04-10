using Celeste.Mod.Anonhelper;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.StrawberryJam2021.Entities;
using Celeste.Mod.VortexHelper.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.JackalHelper.Entities;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modBubbles : IGameModification
    {

        private static Type DirectionalBoosterType;
        private static Type VanillaBoosterType;
        private static Type OneUseBoosterType;
        private static Type WormholeBoosterType;
        private static Type WormholeBoosterTypeCommunal;
        private static Type CoreBoosterType;

        private static Type PurpleBoosterType;
        private static FieldInfo PurpleBoosterSpriteField;
        private static FieldInfo PurpleBoosterOutlineField;
        private static Hook PurpleBoosterUpdateHook;
        private static Hook PurpleBoosterRenderHook;
        private delegate void orig_Update(PurpleBooster self);
        private delegate void orig_Render(PurpleBooster self);

        public override void Load()
        {
            DirectionalBoosterType = typeof(DirectionalBooster);
            VanillaBoosterType = typeof(Booster);
            OneUseBoosterType = typeof(OneUseBooster);
            WormholeBoosterType = CelesteArchipelagoModule.FindType("Celeste.Mod.StrawberryJam2021.Entities.WormholeBooster");
            WormholeBoosterTypeCommunal = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.Entities.StrawberryJam.WormholeBooster");
            PurpleBoosterType = typeof(PurpleBooster);
            CoreBoosterType = typeof(CoreBooster);

            PurpleBoosterSpriteField = PurpleBoosterType.GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic);
            PurpleBoosterOutlineField = PurpleBoosterType.GetField("outline", BindingFlags.Instance | BindingFlags.NonPublic);

            PurpleBoosterUpdateHook = new Hook(PurpleBoosterType.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public),
                typeof(modBubbles).GetMethod(nameof(PurpleBoosterUpdate), BindingFlags.Static | BindingFlags.NonPublic));
            PurpleBoosterRenderHook = new Hook(PurpleBoosterType.GetMethod("Render", BindingFlags.Instance | BindingFlags.Public),
                typeof(modBubbles).GetMethod(nameof(PurpleBoosterRender), BindingFlags.Static | BindingFlags.NonPublic));

            On.Celeste.Booster.Render += modBooster_Render;
            On.Celeste.Booster.Update += modBooster_Update;
        }

        public override void Unload()
        {
            On.Celeste.Booster.Render -= modBooster_Render;
            On.Celeste.Booster.Update -= modBooster_Update;

            PurpleBoosterRenderHook?.Dispose();
            PurpleBoosterUpdateHook?.Dispose();
            PurpleBoosterRenderHook = null;
            PurpleBoosterUpdateHook = null;
        }

        private static void modBooster_Render(On.Celeste.Booster.orig_Render orig, Booster self)
        {
            if (!isEnabled(self) && CelesteArchipelagoModule.shouldModMechanics)
            {
                self.sprite.Visible = false;
                self.outline.Visible = true;
                self.respawnTimer = 0.1f;
            }
            orig(self);
        }

        private static void modBooster_Update(On.Celeste.Booster.orig_Update orig, Booster self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            self.Ch9HubBooster = false;
            self.Ch9HubTransition = false;

            if (!isEnabled(self))
            {
                if (self.outline.Scene == null)
                {
                    self.Scene.Add(self.outline);
                }
                self.Collidable = false;
            }
        }

        private static bool isEnabled(Booster self)
        {
            return self.red && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_BUBBLES) && self.GetType() == VanillaBoosterType ||
                !self.red && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_BUBBLES) && self.GetType() == VanillaBoosterType ||
                !self.red && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_BUBBLES) && self.GetType() == OneUseBoosterType ||
                !self.red && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_BUBBLES) && self.GetType() == OneUseBoosterType ||
                isInHoneyZip() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.HONEY_BUBBLES) && self.GetType() == DirectionalBoosterType ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.WORMHOLE_BUBBLE) && self.GetType() == WormholeBoosterType ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.WORMHOLE_BUBBLE) && self.GetType() == WormholeBoosterTypeCommunal ||
                self.red && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_BUBBLES) && self.GetType() == CoreBoosterType ||
                !self.red && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_BUBBLES) && self.GetType() == CoreBoosterType;
        }

        private static bool isInHoneyZip()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/2-Intermediate/GlowWoomii") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("cp2-3-glowwoomii") ||
                SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/0-Lobbies/2-Intermediate") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("heartside_TiltTheStars");
        }


        private static void PurpleBoosterUpdate(orig_Update orig, PurpleBooster self)
        {
            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_REBOUND_BOOSTER) && CelesteArchipelagoModule.shouldModMechanics)
            {
                Sprite sprite = (Sprite)PurpleBoosterSpriteField.GetValue(self);
                Entity outline = (Entity)PurpleBoosterOutlineField.GetValue(self);
                sprite.Visible = false;
                outline.Visible = true;
                PurpleBoosterSpriteField.SetValue(self, sprite);
                PurpleBoosterOutlineField.SetValue(self, outline);
            }
            orig(self);
        }

        private static void PurpleBoosterRender(orig_Render orig, PurpleBooster self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_REBOUND_BOOSTER))
            {
                self.Collidable = true;
            }
            else
            {
                self.Collidable = false;
            }
        }
    }
}
