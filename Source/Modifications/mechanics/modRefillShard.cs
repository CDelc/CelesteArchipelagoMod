using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.StrawberryJam2021.Entities;
using Microsoft.Xna.Framework;
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
    internal class modRefillShard : IGameModification
    {
        private static Type RefillShardType;
        private static Hook hookRender;

        private delegate void orig_Render(RefillShard self);

        private static FieldInfo SpriteField;

        public override void Load()
        {

            RefillShardType = CelesteArchipelagoModule.FindType("Celeste.Mod.StrawberryJam2021.Entities.RefillShard");

            SpriteField = RefillShardType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo renderMethod = RefillShardType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            hookRender = new Hook(renderMethod, typeof(modRefillShard).GetMethod(nameof(modRender), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            hookRender?.Dispose();
            hookRender = null;
        }

        private static void modRender(orig_Render orig, RefillShard self)
        {
            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTAL_SHARDS) & CelesteArchipelagoModule.shouldModMechanics)
            {
                ((Monocle.Image)SpriteField.GetValue(self)).SetColor(new Color(200, 0, 0, 0.5f));
                self.Collidable = false;
            }
            orig(self);
        }
    }
}
