using Celeste.Mod.CavernHelper;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.StrawberryJam2021.Entities;
using Celeste.Mod.VortexHelper.Entities;
using FactoryHelper.Entities;
using FlaglinesAndSuch;
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
    internal class modActor : IGameModification
    {
        private static Type TheoCrystalType;
        private static Type BowlPufferType;
        private static Type CrystalBombType;
        private static Type TripleBoostFlowerType;
        private static Type VanillaJelly;
        private static Type PlatformJellyType;
        private static Type SwitchCrateType;
        private static Type RespawningJellyFishType;
        private static Type ThrowBoxType;
        private static Type BatteryType;
        private static Type PropellerBoxType;
        private static Type AntiGravJellyType;
        private static Type CustomGliderType;

        private static Hook TheoCrystalHook;
        private static Hook BowlPufferHook;
        private static Hook CrystalBombHook;
        private static Hook TripleBoostFlowerHook;
        private static Hook VanillaJellyHook;
        private static Hook PlatformJellyHook;
        private static Hook SwitchCrateHook;
        private static Hook RespawningJellyFishHook;
        private static Hook ThrowBoxHook;
        private static Hook BatteryHook;
        private static Hook PropellerBoxHook;
        private static Hook AntiGravJellyHook;
        private static Hook CustomGliderHook;

        private static FieldInfo RidablePlatformField;
        private static FieldInfo VanillaDestroyed;
        private static FieldInfo PlatformDestroyed;
        private static FieldInfo PropellerChargesField;

        private delegate void orig_Update(Actor self);

        public override void Load()
        {
            TheoCrystalType = typeof(TheoCrystal);
            BowlPufferType = typeof(BowlPuffer);
            CrystalBombType = typeof(CrystalBomb);
            TripleBoostFlowerType = CelesteArchipelagoModule.FindType("Celeste.Mod.StrawberryJam2021.Entities.TripleBoostFlower");
            VanillaJelly = typeof(Glider);
            PlatformJellyType = typeof(PlatformJelly);
            SwitchCrateType = typeof(SwitchCrate);
            RespawningJellyFishType = typeof(RespawningJellyfish);
            ThrowBoxType = typeof(ThrowBox);
            BatteryType = typeof(Batteries.Battery);
            PropellerBoxType = CelesteArchipelagoModule.FindType("Celeste.Mod.PandorasBox.PropellerBox");
            AntiGravJellyType = CelesteArchipelagoModule.FindType("Celeste.Mod.StrawberryJam2021.Entities.SkyLantern");
            CustomGliderType = typeof(CustomGlider);

            RidablePlatformField = PlatformJellyType.GetField("ridablePlatform", BindingFlags.NonPublic | BindingFlags.Instance);
            VanillaDestroyed = VanillaJelly.GetField("destroyed", BindingFlags.NonPublic | BindingFlags.Instance);
            PlatformDestroyed = PlatformJellyType.GetField("destroyed", BindingFlags.NonPublic | BindingFlags.Instance);
            PropellerChargesField = PropellerBoxType.GetField("MaxCharges", BindingFlags.Public | BindingFlags.Instance);

            BindingFlags updateFlags = BindingFlags.Instance | BindingFlags.Public;
            MethodInfo hookMethod = typeof(modActor).GetMethod(nameof(modActor_Update), BindingFlags.NonPublic | BindingFlags.Static);

            TheoCrystalHook = new Hook(TheoCrystalType.GetMethod("Update", updateFlags), hookMethod);
            BowlPufferHook = new Hook(BowlPufferType.GetMethod("Update", updateFlags), hookMethod);
            CrystalBombHook = new Hook(CrystalBombType.GetMethod("Update", updateFlags), hookMethod);
            TripleBoostFlowerHook = new Hook(TripleBoostFlowerType.GetMethod("Update", updateFlags), hookMethod);
            VanillaJellyHook = new Hook(VanillaJelly.GetMethod("Update", updateFlags), hookMethod);
            PlatformJellyHook = new Hook(PlatformJellyType.GetMethod("Update", updateFlags), hookMethod);
            SwitchCrateHook = new Hook(SwitchCrateType.GetMethod("Update", updateFlags), hookMethod);
            RespawningJellyFishHook = new Hook(RespawningJellyFishType.GetMethod("Update", updateFlags), hookMethod);
            ThrowBoxHook = new Hook(ThrowBoxType.GetMethod("Update", updateFlags), hookMethod);
            BatteryHook = new Hook(BatteryType.GetMethod("Update", updateFlags), hookMethod);
            PropellerBoxHook = new Hook(PropellerBoxType.GetMethod("Update", updateFlags), hookMethod);
            AntiGravJellyHook = new Hook(AntiGravJellyType.GetMethod("Update", updateFlags), hookMethod);
            CustomGliderHook = new Hook(CustomGliderType.GetMethod("Update", updateFlags), hookMethod);

        }

        public override void Unload()
        {
            TheoCrystalHook?.Dispose();
            BowlPufferHook?.Dispose();
            CrystalBombHook?.Dispose();
            TripleBoostFlowerHook?.Dispose();
            VanillaJellyHook?.Dispose();
            PlatformJellyHook?.Dispose();
            SwitchCrateHook?.Dispose();
            RespawningJellyFishHook?.Dispose();
            ThrowBoxHook?.Dispose();
            BatteryHook?.Dispose();
            PropellerBoxHook?.Dispose();
            AntiGravJellyHook?.Dispose();

            TheoCrystalHook = null;
            BowlPufferHook = null;
            CrystalBombHook = null;
            TripleBoostFlowerHook = null;
            VanillaJellyHook = null;
            PlatformJellyHook = null;
            SwitchCrateHook = null;
            RespawningJellyFishHook = null;
            ThrowBoxHook = null;
            BatteryHook = null;
            PropellerBoxHook = null;
            AntiGravJellyHook = null;
        }

        private static void modActor_Update(orig_Update orig, Actor self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            FieldInfo spriteField = self.GetType() == ThrowBoxType ? getImageField((ThrowBox)self) : getSpriteField(self);
            if (!isActive(self))
            {
                Disable(self, spriteField);
            }
            else if(!isDestroyedJelly(self))
            {
                Enable(self, spriteField);
            }
        }

        private static bool isActive(Actor self)
        {
            int propellerUses = 0;
            if(self.GetType() == PropellerBoxType)
            {
                propellerUses = (int)PropellerChargesField.GetValue(self);
            }
            return self.GetType() == TheoCrystalType && !isStrawberryOrchard() && !isBellyoftheBeast() && !isSolex() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.THEO_CRYSTAL) ||
                self.GetType() == TheoCrystalType && isStrawberryOrchard() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.STRAWBERRY_JAM) ||
                self.GetType() == TheoCrystalType && isBellyoftheBeast() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CANNON_BALL) ||
                self.GetType() == TheoCrystalType && isSolex() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_ROCK) ||
                self.GetType() == BowlPufferType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BOWL_PUFFER) ||
                self.GetType() == CrystalBombType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CRYSTAL_BOMB) ||
                self.GetType() == TripleBoostFlowerType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TRIPLE_BOOST_FLOWER) ||
                self.GetType() == VanillaJelly && !isStormRunner() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.JELLYFISH) ||
                self.GetType() == VanillaJelly && isStormRunner() && !isStormRunnerJellyException(self) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.JELLYFISH_CRYSTAL) ||
                self.GetType() == VanillaJelly && isStormRunner() && isStormRunnerJellyException(self) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.JELLYFISH) ||
                self.GetType() == PlatformJellyType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PURPLE_JELLYFISH) ||
                self.GetType() == SwitchCrateType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SWITCH_CRATE) ||
                self.GetType() == RespawningJellyFishType && !isNelumbo() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.JELLYFISH) ||
                self.GetType() == RespawningJellyFishType && isNelumbo() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_FLYING_LANTERN) ||
                self.GetType() == CustomGliderType && !isNelumbo() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.JELLYFISH) ||
                self.GetType() == CustomGliderType && isNelumbo() && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_FLYING_LANTERN) ||
                self.GetType() == ThrowBoxType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.THROW_BOX) ||
                self.GetType() == BatteryType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BATTERY) ||
                self.GetType() == PropellerBoxType && propellerUses == 3 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.RED_PROPELLER_BLOCK) ||
                self.GetType() == PropellerBoxType && propellerUses != 3 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_PROPELLER_BLOCK) ||
                self.GetType() == AntiGravJellyType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SKY_LANTERN);
        }

        private static FieldInfo getSpriteField(Actor self)
        {
            Type type = self.GetType();
            if(type == RespawningJellyFishType)
            {
                return typeof(Glider).GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            FieldInfo spriteField = type.GetField(type == BowlPufferType ? "puffer" : type == PropellerBoxType ? "chargeSprites" : "sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            return spriteField;
        }

        private static FieldInfo getImageField(ThrowBox self)
        {
            FieldInfo ImageField = typeof(ThrowBox).GetField("_image", BindingFlags.NonPublic | BindingFlags.Instance);
            return ImageField;
        }

        private static void Disable(Actor self, FieldInfo spriteField)
        {
            if(self.GetType() == ThrowBoxType)
            {
                Image image = (Image)spriteField.GetValue(self);
                self.Collidable = false;
                image.Color.R = (byte)(0.5f * 255);
                image.Color.G = (byte)(0.5f * 255);
                image.Color.B = (byte)(0.5f * 255);
                image.Color.A = (byte)(0.1f * 255);
                spriteField.SetValue(self, image);
            }
            else if(self.GetType() == PropellerBoxType){
                List<Sprite> sprites = (List<Sprite>)spriteField.GetValue(self);
                self.Collidable = false;
                foreach(Sprite sprite in sprites)
                {
                    sprite.Color.R = (byte)(0.5f * 255);
                    sprite.Color.G = (byte)(0.5f * 255);
                    sprite.Color.B = (byte)(0.5f * 255);
                    sprite.Color.A = (byte)(0.1f * 255);
                }
                spriteField.SetValue(self, sprites);
            }
            else
            {
                if (self.GetType() == PlatformJellyType)
                {
                    JumpThru platform = null;
                    platform = (JumpThru)RidablePlatformField.GetValue(self);
                    platform.Collidable = false;
                    RidablePlatformField.SetValue(self, platform);
                }
                Sprite sprite = (Sprite)spriteField.GetValue(self);
                self.Collidable = false;
                sprite.Color.R = (byte)(0.5f * 255);
                sprite.Color.G = (byte)(0.5f * 255);
                sprite.Color.B = (byte)(0.5f * 255);
                sprite.Color.A = (byte)(0.1f * 255);
                spriteField.SetValue(self, sprite);
            }
        }

        private static void Enable(Actor self, FieldInfo spriteField)
        {
            if (self.GetType() == ThrowBoxType)
            {
                Image image = (Image)spriteField.GetValue(self);
                self.Collidable = true;
                image.Color.R = (byte)255;
                image.Color.G = (byte)255;
                image.Color.B = (byte)255;
                image.Color.A = (byte)255;
                spriteField.SetValue(self, image);
            }
            else if (self.GetType() == PropellerBoxType)
            {
                List<Sprite> sprites = (List<Sprite>)spriteField.GetValue(self);
                self.Collidable = true;
                foreach (Sprite sprite in sprites)
                {
                    sprite.Color.R = (byte)255;
                    sprite.Color.G = (byte)255;
                    sprite.Color.B = (byte)255;
                    sprite.Color.A = (byte)255;
                }
                spriteField.SetValue(self, sprites);
            }
            else
            {
                if (self.GetType() == PlatformJellyType)
                {
                    JumpThru platform = null;
                    platform = (JumpThru)RidablePlatformField.GetValue(self);
                    platform.Collidable = true;
                    RidablePlatformField.SetValue(self, platform);
                }
                Sprite sprite = (Sprite)spriteField.GetValue(self);
                self.Collidable = true;
                sprite.Color.R = (byte)255;
                sprite.Color.G = (byte)255;
                sprite.Color.B = (byte)255;
                sprite.Color.A = (byte)255;
                spriteField.SetValue(self, sprite);
            }
        }

        private static bool isDestroyedJelly(Actor self)
        {
            if (self.GetType() == VanillaJelly || self.GetType() == RespawningJellyFishType)
            {
                return (bool)VanillaDestroyed.GetValue(self);
            }
            else if (self.GetType() == PlatformJellyType)
            {
                return (bool)PlatformDestroyed.GetValue(self);
            }
            else return false;
        }

        private static bool isStrawberryOrchard()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/1-Beginner/Jadeturtle") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("cp4_09_heartside_jadeturtle");
        }

        private static bool isBellyoftheBeast()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/5-Grandmaster/CaptainCarpensir") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("d2_08-Aiden");
        }

        private static bool isSolex()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/5-Grandmaster/Soloiini") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("f2_02-Soloiini");
        }

        private static bool isNelumbo()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/5-Grandmaster/tofu") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("e1_06-tofu");
        }

        private static bool isStormRunner()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/4-Expert/LethargicDoggo") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("d02_lethargicdoggo");
        }

        private static bool isStormRunnerJellyException(Actor self)
        {
            if (self.GetType() != VanillaJelly) return false;
            Glider jelly = (Glider)self;
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/4-Expert/LethargicDoggo") &&
                SaveData.Instance.CurrentSession_Safe.Level.Equals("a-10") &&
                jelly.SourceId.ID == 1660;
        }

    }
}
