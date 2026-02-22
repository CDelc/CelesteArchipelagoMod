using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modLockBlock : IGameModification
    {

        static Key animKey = null;

        public override void Load()
        {
            On.Celeste.LockBlock.OnPlayer += modOnPlayer;
        }

        public override void Unload()
        {
            On.Celeste.LockBlock.OnPlayer -= modOnPlayer;
        }

        private static void modOnPlayer(On.Celeste.LockBlock.orig_OnPlayer orig, LockBlock self, Player player)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || self.opening)
            {
                orig(self, player);
                return;
            }

            string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
            AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;
            if (CelesteArchipelagoModule.SaveData.UnlockedKeyDoors.Contains(ArchipelagoMapper.getLockDoorID(SID, mode, self.ID)))
            {
                if (animKey == null || animKey.IsUsed)
                {
                    animKey = new Key(player, new EntityID("0", 0));
                }
                self.SceneAs<Level>().Add(animKey);
                self.SceneAs<Level>().Session.Keys.Add(animKey.ID);

                self.TryOpen(player, animKey.follower);
            }
        }
    }
}
