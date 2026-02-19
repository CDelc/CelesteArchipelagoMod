using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.UI;
using Microsoft.Xna.Framework;
using System;
using static Celeste.Mod.CelesteArchipelago.ArchipelagoData.ArchipelagoManager;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{

    public class RoomDisplay : Monocle.Entity
    {
        public static string CurrentRoom = "";
        public static int RoomDisplayTimer = 0;

        public RoomDisplay()
        {
            base.Y = 196f;
            base.Depth = -101;
            base.Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate;
        }

        public override void Render()
        {
            if (SaveData.Instance == null ||
                SaveData.Instance.CurrentSession_Safe == null ||
                !CelesteArchipelagoModule.Settings.RoomPopups ||
                !CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                return;
            }

            if (SaveData.Instance.CurrentSession_Safe.Level != CurrentRoom && SaveData.Instance.CurrentSession_Safe.Level != "")
            {
                CurrentRoom = SaveData.Instance.CurrentSession_Safe.Level;

                RoomDisplayTimer = 180;
            }

            if (RoomDisplayTimer >= 0)
            {
                float alpha = 1.0f;

                if (RoomDisplayTimer > 150)
                {
                    alpha = (float)(180.0f - RoomDisplayTimer) / 30.0f;
                }
                else if (RoomDisplayTimer < 30)
                {
                    alpha = (float)(RoomDisplayTimer) / 30.0f;
                }

                Color TextColor = new Color(0.96078f, 0.25882f, 0.78431f, alpha);

                ActiveFont.Draw($"Room: {CurrentRoom}", new Vector2(50f, 1030f), new Vector2(0.0f, 0.5f), Vector2.One, TextColor, 5.0f, new Color(0.0f, 0.0f, 0.0f, alpha), 0.0f, new Color(1.0f, 0.0f, 0.0f, 0.0f));
                RoomDisplayTimer--;
            }
        }
    }

    internal class modPlayer : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Player.Update += modPlayer_Update;
            On.Celeste.Player.ClimbCheck += modPlayer_ClimbCheck;
        }

        public override void Unload()
        {
            On.Celeste.Player.Update -= modPlayer_Update;
            On.Celeste.Player.ClimbCheck -= modPlayer_ClimbCheck;
        }

        private bool modPlayer_ClimbCheck(On.Celeste.Player.orig_ClimbCheck orig, Player self, int dir, int yAdd)
        {
            if (CelesteArchipelagoModule.IsInArchipelagoSave && ArchipelagoManager.Instance.randomize_climb && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CLIMB)){
                return false;
            }

            else return orig(self, dir, yAdd);
        }

        private void modPlayer_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            if (CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                HandleMessageQueue(self);
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
                        self.Scene.Add(new ArchipelagoTextBox(message.Text, queueSize > 8 ? 1f : queueSize > 4 ? 2f : 3f));
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
    }
}
