using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.UI;
using Microsoft.Xna.Framework;
using System;
using static Celeste.Mod.CelesteArchipelago.ArchipelagoData.ArchipelagoManager;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{

    internal class modPlayer : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Player.Update += modPlayer_Update;
        }

        public override void Unload()
        {
            On.Celeste.Player.Update -= modPlayer_Update;
        }

        private static void modPlayer_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            if (CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                HandleMessageQueue(self);
                ArchipelagoManager.Instance.CheckCompleteGame();
            }
        }

        private static void HandleMessageQueue(Player self)
        {
            if (ArchipelagoManager.Instance == null || self.Scene == null)
            {
                return;
            }

            var messageQueue = ArchipelagoManager.Instance.MessageQueue;
            int queueSize = messageQueue.Count;

            if (queueSize > 0 && self.Scene.Tracker.GetEntity<ArchipelagoTextBox>() == null)
            {
                self.Scene.Add(new ArchipelagoTextBox());
            }
        }
    }
}
