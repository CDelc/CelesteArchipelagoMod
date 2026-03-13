using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.StrawberryJam2021.Entities;
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
    internal class modDashBoostField : IGameModification
    {
        private static Type DashBoostFieldType;
        private static FieldInfo BoostFieldTextureField;

        private static Hook HookUpdate;
        private delegate void orig_Constructor(DashBoostField self, EntityData data, Vector2 offset);

        public override void Load()
        {
            DashBoostFieldType = typeof(DashBoostField);
            HookUpdate = new Hook(DashBoostFieldType.GetConstructor(new[] { typeof(EntityData), typeof(Vector2) }),
                typeof(modDashBoostField).GetMethod(nameof(modCtr), BindingFlags.Static | BindingFlags.NonPublic));
            BoostFieldTextureField = DashBoostFieldType.GetField("boostFieldTexture", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void Unload()
        {
            HookUpdate?.Dispose();
            HookUpdate = null;
        }

        private static void modCtr(orig_Constructor orig, DashBoostField self, EntityData data, Vector2 offset)
        {
            orig(self, data, offset);
            if(CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_BOOST_FIELD))
            {
                self.Collidable = false;
                ((Image)BoostFieldTextureField.GetValue(self)).Visible = false;
                Image outline = new Image(GFX.Game["objects/booster/outline"]);
                outline.CenterOrigin();
                outline.Color = Color.White * 0.75f;
                outline.Visible = true;
                self.Add(outline);
            }
        }
    }
}
