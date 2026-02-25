using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
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
            On.Celeste.Seeker.ctor_Vector2_Vector2Array += modSeeker_ctor_Vector2_Vector2Array;
            On.Celeste.Seeker.Update += modSeeker_Update;
        }

        public override void Unload()
        {
            On.Celeste.Seeker.ctor_Vector2_Vector2Array -= modSeeker_ctor_Vector2_Vector2Array;
            On.Celeste.Seeker.Update -= modSeeker_Update;
        }

        private static void modSeeker_ctor_Vector2_Vector2Array(On.Celeste.Seeker.orig_ctor_Vector2_Vector2Array orig, Seeker self, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2[] patrolPoints)
        {
            orig(self, position, patrolPoints);

            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SEEKERS))
            {
                self.State.State = 6;
                self.sprite.Play("pulse", false, false);
                self.State.SetCallbacks(6, self.RegenerateUpdate);
            }
        }

        private static void modSeeker_Update(On.Celeste.Seeker.orig_Update orig, Seeker self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SEEKERS))
            {
                self.State.State = 6;
                self.sprite.Play("pulse", false, false);
            }
            else if(CelesteArchipelagoModule.shouldModMechanics)
            {
                self.State.SetCallbacks(6, new Func<int>(self.RegenerateUpdate), new Func<IEnumerator>(self.RegenerateCoroutine), new Action(self.RegenerateBegin), new Action(self.RegenerateEnd));
                if (self.State.state == 6 && (self.State.currentCoroutine == null || self.State.currentCoroutine.Finished))
                {
                    self.State.currentCoroutine.Replace(self.State.coroutines[6]());
                }
            }

            orig(self);
        }
    }
}
