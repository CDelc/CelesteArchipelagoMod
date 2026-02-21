using Archipelago.MultiClient.Net;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    public class OuiConnection : Oui
    {
        public static OuiConnection Instance;
        private TextMenu connectMenu;

        private bool waitingOnConnect = false;
        private Task<LoginFailure> connectTask;

        public OuiConnection()
        {
            Instance = this;
        }

        public override void Update()
        {
            if (connectTask != null && waitingOnConnect)
            {
                if (connectTask.IsCompleted)
                {
                    if (connectTask.Result == null)
                    {
                        BeginGame();
                        waitingOnConnect = false;
                        return;
                    }
                    else
                    {
                        (this.connectMenu.items[6] as TextMenu.Header).Title = "Connection Failed";
                        Logger.Error("AP", "Connection Failed.");
                        connectTask = null;
                        waitingOnConnect = false;
                        return;
                    }
                }
                else
                {
                    (this.connectMenu.items[6] as TextMenu.Header).Title = "Connecting...";
                    return;
                }
            }

            if (base.Selected &&
                !waitingOnConnect &&
                connectMenu != null &&
                connectMenu.Focused &&
                connectMenu.Selection == 7)
            {
                if (Input.MenuConfirm.Pressed)
                {
                    waitingOnConnect = true;
                    connectTask = ArchipelagoManager.Instance.Connect();
                }

                if (Input.MenuCancel.Pressed)
                {
                    Audio.Play("event:/ui/main/button_back");
                    base.Overworld.Goto<OuiMainMenu>();
                }
            }
        }

        public override IEnumerator Enter(Oui from)
        {
            TextInput.OnInput += modTextInput_OnInput;

            RefreshConnectionMenu();
            this.Visible = true;
            this.connectMenu.Visible = true;
            this.connectMenu.Focused = true;

            yield return null;
        }

        public override IEnumerator Leave(Oui next)
        {
            TextInput.OnInput -= modTextInput_OnInput;

            yield return Everest.SaveSettings();

            RefreshConnectionMenu();
            this.Visible = false;
            this.connectMenu.Visible = false;
            this.connectMenu.RemoveSelf();
            this.connectMenu = null;
        }

        private void modTextInput_OnInput(char obj)
        {
            if (this.connectMenu.Selection != 7)
            {
                string currentButtonText = (this.connectMenu.items[this.connectMenu.Selection] as TextMenu.Button).Label;
                if (obj == 8 && currentButtonText.Length > 0)
                {
                    (this.connectMenu.items[this.connectMenu.Selection] as TextMenu.Button).Label = currentButtonText.Remove(currentButtonText.Length - 1);
                }
                else if (obj >= 32 && obj <= 126)
                {
                    (this.connectMenu.items[this.connectMenu.Selection] as TextMenu.Button).Label += obj;
                }
                else if (obj == 13)
                {
                    this.connectMenu.Selection += 2;
                }

                Core.CoreModule.Settings.DebugConsole.Keys.Remove(Microsoft.Xna.Framework.Input.Keys.OemPeriod);
            }

            CelesteArchipelagoModule.Settings.Address = (this.connectMenu.items[1] as TextMenu.Button).Label;
            CelesteArchipelagoModule.Settings.PlayerName = (this.connectMenu.items[3] as TextMenu.Button).Label;
            CelesteArchipelagoModule.Settings.Password = (this.connectMenu.items[5] as TextMenu.Button).Label;
        }

        public TextMenu CreateConnectMenu()
        {
            Type settings = typeof(CelesteArchipelagoModule);

            TextMenu retMenu = new TextMenu();
            retMenu.BatchMode = true;
            retMenu.CompactWidthMode = true;

            retMenu.Add(new TextMenu.Header("Address"));
            retMenu.Add(new TextMenu.Button(CelesteArchipelagoModule.Settings.Address));
            retMenu.Add(new TextMenu.Header("Player Name"));
            retMenu.Add(new TextMenu.Button(CelesteArchipelagoModule.Settings.PlayerName));
            retMenu.Add(new TextMenu.Header("Password"));
            retMenu.Add(new TextMenu.Button(CelesteArchipelagoModule.Settings.Password));
            retMenu.Add(new TextMenu.Header(""));

            TextMenu.Button connectButton = new TextMenu.Button("Connect");
            retMenu.Add(connectButton);

            return retMenu;
        }

        public void RefreshConnectionMenu()
        {
            int menuSelection = -1;
            Vector2 menuPos = Vector2.Zero;

            if (this.connectMenu != null)
            {
                menuSelection = this.connectMenu.Selection;
                menuPos = this.connectMenu.Position;
                Scene.Remove(connectMenu);
            }

            this.connectMenu = CreateConnectMenu();

            if (menuSelection != -1)
            {
                this.connectMenu.Position = menuPos;
                this.connectMenu.Selection = menuSelection;
            }

            Scene.Add(connectMenu);
        }

        public void BeginGame()
        {
            SaveData.TryDelete(Constants.SAVE_ID);
            SaveData.TryDeleteModSaveData(Constants.SAVE_ID);
            SaveData.Start(new SaveData
            {
                Name = CelesteArchipelagoModule.Settings.PlayerName,
                AssistMode = false,
                VariantMode = false
            }, Constants.SAVE_ID);


            if (SaveData.Instance != null)
            {
                foreach (LevelSetStats levelSet in SaveData.Instance.LevelSets)
                {
                    if (levelSet.Name == "Celeste")
                    {
                        for (int i = 0; i < SaveData.Instance.Areas_Safe.Count; i++)
                        {
                            for (int j = 0; j < SaveData.Instance.Areas_Safe[i].Modes.Length; j++)
                            {
                                SaveData.Instance.Areas_Safe[i].Modes[j].Completed = true;
                            }
                        }
                    }
                    else if(levelSet.Name == "StrawberryJam2021/0-Lobbies")
                    {
                        levelSet.UnlockedAreas = 6;
                    }
                }

                SaveData.Instance.UnlockedAreas = 10;
                //CelesteArchipelagoModule.Log("------------------------------------------------------");

                //CelesteArchipelagoModule.Log($"{SaveData.Instance.UnlockedModes}");
                //CelesteArchipelagoModule.Log($"{AreaData.Areas[1].Interlude_Safe}");
                //CelesteArchipelagoModule.Log($"{AreaData.Areas[1].HasMode(AreaMode.CSide)}");
                //CelesteArchipelagoModule.Log($"{Celeste.PlayMode != Celeste.PlayModes.Event}");
            }

            (Scene as Overworld).Goto<OuiChapterSelect>();
        }
    }
}
