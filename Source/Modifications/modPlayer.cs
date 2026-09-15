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
                if (messageQueue.TryDequeue(out ArchipelagoMessage message))
                {
                    if (ShouldShowMessage(message))
                    {
                        float duration = queueSize > 8 ? 1f : queueSize > 4 ? 2f : 3f;
                        Color color = GetMessageColor(message);
                        self.Scene.Add(new ArchipelagoTextBox(message.Text, duration, color));
                        Logger.Verbose(Constants.LOG_PREFIX, message.Text);
                    }
                }
            }
        }

        private static bool ShouldShowMessage(ArchipelagoMessage message)
        {
            if (message.Type == ArchipelagoMessage.MessageType.Server)
            {
                return CelesteArchipelagoModule.Settings.ServerMessages;
            }
            return true;
        }

        private static Color GetMessageColor(ArchipelagoMessage message)
        {
            switch (message.Type)
            {
                case ArchipelagoMessage.MessageType.ItemReceive:
                    return Color.Red;
                case ArchipelagoMessage.MessageType.ItemSend:
                    return ArchipelagoManager.GetItemColor(message.Flags);
                case ArchipelagoMessage.MessageType.ItemHint:
                    return Color.Orange;
                case ArchipelagoMessage.MessageType.Server:
                    return Color.LightGray;
                default:
                    return Color.White;
            }
        }
    }
}
