using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modSummitGem : IGameModification
    {
        public override void Load()
        {
            On.Celeste.SummitGem.OnPlayer += modOnPlayer;
            On.Celeste.SummitGemManager.Routine += modRoutine;
        }

        public override void Unload()
        {
            On.Celeste.SummitGem.OnPlayer -= modOnPlayer;
            On.Celeste.SummitGemManager.Routine -= modRoutine;
        }

        private static IEnumerator modRoutine(On.Celeste.SummitGemManager.orig_Routine orig, SummitGemManager self)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                yield return orig(self);
            }

            Level level = self.Scene as Level;
            if (level.Session.HeartGem)
            {
                foreach (SummitGemManager.Gem gem2 in self.gems)
                {
                    gem2.Sprite.RemoveSelf();
                }
                self.gems.Clear();
                yield break;
            }
            for (; ; )
            {
                Player entity = self.Scene.Tracker.GetEntity<Player>();
                if (entity != null && (entity.Position - self.Position).Length() < 64f)
                {
                    break;
                }
                yield return null;
            }
            yield return 0.5f;
            bool alreadyHasHeart = level.Session.OldStats.Modes[0].HeartGem;
            int broken = 0;
            int index = 0;
            foreach (SummitGemManager.Gem gem in self.gems)
            {
                long gemSaveID = ArchipelagoMapper.getGemItemID("Celeste/7-Summit", AreaMode.Normal, ArchipelagoMapper.summitGemIndexReverseMapping[index]);
                bool flag = CelesteArchipelagoModule.SaveData.SummitGemsUnlocked.Contains(gemSaveID);
                int num;
                if (flag)
                {
                    if (index == 0)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_1", gem.Position);
                    }
                    else if (index == 1)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_2", gem.Position);
                    }
                    else if (index == 2)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_3", gem.Position);
                    }
                    else if (index == 3)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_4", gem.Position);
                    }
                    else if (index == 4)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_5", gem.Position);
                    }
                    else if (index == 5)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_6", gem.Position);
                    }
                    gem.Sprite.Play("spin", false, false);
                    while (gem.Sprite.CurrentAnimationID == "spin")
                    {
                        gem.Bloom.Alpha = Calc.Approach(gem.Bloom.Alpha, 1f, Engine.DeltaTime * 3f);
                        if (gem.Bloom.Alpha > 0.5f)
                        {
                            gem.Shake = Calc.Random.ShakeVector();
                        }
                        gem.Sprite.Y -= Engine.DeltaTime * 8f;
                        gem.Sprite.Scale = Vector2.One * (1f + gem.Bloom.Alpha * 0.1f);
                        yield return null;
                    }
                    yield return 0.2f;
                    level.Shake(0.3f);
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                    for (int i = 0; i < 20; i++)
                    {
                        level.ParticlesFG.Emit(SummitGem.P_Shatter, gem.Position + new Vector2((float)Calc.Random.Range(-8, 8), (float)Calc.Random.Range(-8, 8)), SummitGem.GemColors[index], Calc.Random.NextFloat(6.2831855f));
                    }
                    num = broken;
                    broken = num + 1;
                    gem.Bloom.RemoveSelf();
                    gem.Sprite.RemoveSelf();
                    yield return 0.25f;
                }
                num = index;
                index = num + 1;
            }

            if (broken >= 6)
            {
                HeartGem heart = self.Scene.Entities.FindFirst<HeartGem>();
                if (heart != null)
                {
                    Audio.Play("event:/game/07_summit/gem_unlock_complete", heart.Position);
                    yield return 0.1f;
                    Vector2 from = heart.Position;
                    float p = 0f;
                    while (p < 1f && heart.Scene != null)
                    {
                        heart.Position = Vector2.Lerp(from, self.Position + new Vector2(0f, -16f), Ease.CubeOut(p));
                        yield return null;
                        p += Engine.DeltaTime;
                    }
                    from = default(Vector2);
                }
                heart = null;
            }
            yield break;
        }

        private static void modOnPlayer(On.Celeste.SummitGem.orig_OnPlayer orig, SummitGem self, Player player)
        {
            if (player.DashAttacking && CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
                AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;
                long locationID = ArchipelagoMapper.getGemLocationID(SID, mode, self.GID);
                CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);
                CelesteArchipelagoModule.Log($"Collected Gem {SID} {mode} {self.GID.Level} {self.GID.ID}, mapping to location id {locationID}");
            }
            orig(self, player);
        }
    }
}
