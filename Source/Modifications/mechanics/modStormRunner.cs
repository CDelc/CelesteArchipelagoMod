using Celeste.Mod.Anonhelper;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
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
    internal class modStormRunner : IGameModification
    {

        private static Type CloudRefillType;
        private static Type JellyRefillType;

        private static FieldInfo CloudSprite;
        private static FieldInfo CloudFlash;
        private static FieldInfo CloudOutline;
        private static FieldInfo JellySprite;
        private static FieldInfo JellyFlash;
        private static FieldInfo JellyOutline;

        private static Hook CloudRender;
        private static Hook JellyRender;

        private delegate void orig_CloudRender(CloudRefill self);
        private delegate void orig_JellyRender(JellyRefill self);

        private static BindingFlags privateLookup = BindingFlags.Instance | BindingFlags.NonPublic;
        private static BindingFlags privateStatic = BindingFlags.Static | BindingFlags.NonPublic;
        private static BindingFlags publicLookup = BindingFlags.Public | BindingFlags.Instance;
        
        public override void Load()
        {
            CloudRefillType = typeof(CloudRefill);
            JellyRefillType = typeof(JellyRefill);

            CloudSprite = CloudRefillType.GetField("sprite", privateLookup);
            CloudFlash = CloudRefillType.GetField("flash", privateLookup);
            CloudOutline = CloudRefillType.GetField("outline", privateLookup);
            JellySprite = JellyRefillType.GetField("sprite", privateLookup);
            JellyFlash = JellyRefillType.GetField("flash", privateLookup);
            JellyOutline = JellyRefillType.GetField("outline", privateLookup);

            CloudRender = new Hook(CloudRefillType.GetMethod("Render", publicLookup), typeof(modStormRunner).GetMethod(nameof(modCloudRender), privateStatic));
            JellyRender = new Hook(JellyRefillType.GetMethod("Render", publicLookup), typeof(modStormRunner).GetMethod(nameof(modJellyRender), privateStatic));
        }

        public override void Unload()
        {
            CloudRender?.Dispose();
            JellyRender?.Dispose();
            CloudRender = null;
            JellyRender = null;
        }

        private static void modCloudRender(orig_CloudRender orig, CloudRefill self)
        {
            if(CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CLOUD_CRYSTAL))
            {
                self.Collidable = false;
                ((Sprite)CloudSprite.GetValue(self)).RemoveSelf();
                ((Sprite)CloudFlash.GetValue(self)).RemoveSelf();
                ((Image)CloudOutline.GetValue(self)).Visible = true;
            }
            else
            {
                self.Collidable = true;
            }
            orig(self);
        }

        private static void modJellyRender(orig_JellyRender orig, JellyRefill self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.JELLYFISH_CRYSTAL))
            {
                self.Collidable = false;
                ((Sprite)JellySprite.GetValue(self)).RemoveSelf();
                ((Sprite)JellyFlash.GetValue(self)).RemoveSelf();
                ((Image)JellyOutline.GetValue(self)).Visible = true;
            }
            else
            {
                self.Collidable = true;
            }
            orig(self);
        }
    }
}
