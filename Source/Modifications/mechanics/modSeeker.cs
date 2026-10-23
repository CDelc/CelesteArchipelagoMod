using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modSeeker : IGameModification
    {
        public override void Load()
        {

            //On.Celeste.Seeker.ctor_Vector2_Vector2Array += modSeeker_ctor_Vector2_Vector2Array;
            On.Celeste.Seeker.Update += modSeeker_Update;
            On.Celeste.SeekerEffectsController.Update += modSeekerEffects_Update;
        }

        public override void Unload()
        {
            //On.Celeste.Seeker.ctor_Vector2_Vector2Array -= modSeeker_ctor_Vector2_Vector2Array;
            On.Celeste.Seeker.Update -= modSeeker_Update;
            On.Celeste.SeekerEffectsController.Update += modSeekerEffects_Update;
        }

        //private static void modSeeker_ctor_Vector2_Vector2Array(On.Celeste.Seeker.orig_ctor_Vector2_Vector2Array orig, Seeker self, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2[] patrolPoints)
        //{
        //    orig(self, position, patrolPoints);

        //    if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SEEKERS))
        //    {
        //        self.State.SetCallbacks(6, self.RegenerateUpdate);
        //    }
        //}

        private static void modSeeker_Update(On.Celeste.Seeker.orig_Update orig, Seeker self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SEEKERS))
            {
                self.Collidable = false;
                self.sprite.Color = Color.Red * 0.3f;
            }
            else if(CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                //self.State.SetCallbacks(6, new Func<int>(self.RegenerateUpdate), new Func<IEnumerator>(self.RegenerateCoroutine), new Action(self.RegenerateBegin), new Action(self.RegenerateEnd));
                //if (self.State.state == 6 && (self.State.currentCoroutine == null || self.State.currentCoroutine.Finished))
                //{
                //    self.State.currentCoroutine.Replace(self.State.coroutines[6]());
                //}
            }

        }

        private static void modSeekerEffects_Update(On.Celeste.SeekerEffectsController.orig_Update orig, SeekerEffectsController self)
        {
            if(CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SEEKERS))
            {
                self.enabled = false;
            }
            orig(self);
        }
    }
}
