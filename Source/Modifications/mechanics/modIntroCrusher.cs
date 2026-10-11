using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
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
    internal class modIntroCrusher : IGameModification
    {
        private static Type FlagIntroCrusherType;
        FieldInfo FlagCrusherStart;
        FieldInfo FlagCrusherEnd;
        public override void Load()
        {
            FlagIntroCrusherType = typeof(FlagIntroCrusher);
            On.Celeste.IntroCrusher.Added += modAdded;
            On.Monocle.Entity.Added += modFlagAdded;

            FlagCrusherStart = FlagIntroCrusherType.GetField("start", BindingFlags.NonPublic | BindingFlags.Instance);
            FlagCrusherEnd = FlagIntroCrusherType.GetField("end", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override void Unload()
        {
            On.Celeste.IntroCrusher.Added -= modAdded;
            On.Monocle.Entity.Added += modFlagAdded;
        }

        private void modFlagAdded(On.Monocle.Entity.orig_Added orig, Entity _self, Scene scene)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && _self is FlagIntroCrusher self && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.INTRO_CRUSHER))
            {
                FlagCrusherEnd.SetValue(self, FlagCrusherStart.GetValue(self));
                Constants.DrawDisabledRect(self.Collider);
            }
            orig(_self, scene);
        }

        private void modAdded(On.Celeste.IntroCrusher.orig_Added orig, IntroCrusher self, Scene scene)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.INTRO_CRUSHER))
            {
                self.end = self.start;
                self.Position = self.start;
                self.tilegrid.Color = Color.Red;
            }
            orig(self, scene);
        }
    }
}
