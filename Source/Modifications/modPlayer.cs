using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.UI;
using Microsoft.Xna.Framework;
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
            if (SaveData.Instance == null || SaveData.Instance.CurrentSession_Safe == null || !CelesteArchipelagoModule.Settings.RoomPopups)
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
            On.Celeste.Player.Pickup += modPlayer_Pickup;
        }

        public override void Unload()
        {
            On.Celeste.Player.Update -= modPlayer_Update;
            On.Celeste.Player.ClimbCheck -= modPlayer_ClimbCheck;
            On.Celeste.Player.Pickup -= modPlayer_Pickup;
        }

        private void modPlayer_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            HandleMessageQueue(self);
        }

        private bool modPlayer_ClimbCheck(On.Celeste.Player.orig_ClimbCheck orig, Player self, int dir, int yAdd)
        {
            if (!IsGrabEnabled())
                return false;
            return orig(self, dir, yAdd);
        }

        private bool modPlayer_Pickup(On.Celeste.Player.orig_Pickup orig, Player self, Holdable pickup)
        {
            if (!IsGrabEnabled())
                return false;
            return orig(self, pickup);
        }

        private static bool IsGrabEnabled()
        {
            if (!ArchipelagoManager.Instance.Ready || !ArchipelagoManager.Instance.randomize_climb)
                return true;

            var saveData = CelesteArchipelagoModule.SaveData;
            if (saveData == null)
                return true;

            return saveData.Mechanics.TryGetValue(Constants.MECHANIC_CLIMB, out bool unlocked) && unlocked;
        }

        private static void HandleMessageQueue(Player self)
        {
            if (ArchipelagoManager.Instance.MessageQueue.Count > 0)
            {
                if (self.Scene.Tracker.GetEntity<ArchipelagoTextBox>() == null)
                {
                    ArchipelagoMessage message = ArchipelagoManager.Instance.MessageQueue.Dequeue();

                    if (ShouldShowMessage(message))
                    {
                        self.Scene.Add(new ArchipelagoTextBox(message.Text));
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
