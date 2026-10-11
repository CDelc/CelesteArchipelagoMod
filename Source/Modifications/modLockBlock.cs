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

        public override void Load()
        {
            On.Celeste.LockBlock.OnPlayer += modOnPlayer;
            On.Monocle.Entity.Render += modRender;
        }

        public override void Unload()
        {
            On.Celeste.LockBlock.OnPlayer -= modOnPlayer;
            On.Monocle.Entity.Render -= modRender;
        }

        private void modRender(On.Monocle.Entity.orig_Render orig, Monocle.Entity self)
        {
            orig(self);
            if(self is LockBlock block && CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
                AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;
                if (CelesteArchipelagoModule.SaveData.UnlockedKeyDoors.Contains(ArchipelagoMapper.getLockDoorID(SID, mode, block.ID)))
                {
                    Constants.DrawDisabledRect(block.Collider, new Microsoft.Xna.Framework.Color(255, 255, 0) * 0.2f);
                }
            }
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
                //if (animKey == null || animKey.IsUsed)
                //{
                //    animKey = new Key(player, new EntityID("0", 0));
                //}
                //self.SceneAs<Level>().Add(animKey);
                //self.SceneAs<Level>().Session.Keys.Add(animKey.ID);

                //self.TryOpen(player, animKey.follower);
                self.Collidable = false;
                self.Visible = false;
            }
        }
    }
}
