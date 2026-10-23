using Archipelago.MultiClient.Net;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.Modifications;
using Celeste.Mod.CollabUtils2;
using Celeste.Mod.StrawberryJam2021;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    public class OuiConnection : Oui
    {
        public static OuiConnection Instance;
        private TextMenu connectMenu;
        private int pendingSelection = -1;
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
                    else if (ArchipelagoManager.Instance.VersionError)
                    {
                        (this.connectMenu.items[6] as TextMenu.Header).Title = "Incompatible APWorld Version";
                        Logger.Error(Constants.LOG_PREFIX, "Incompatible APWorld Version");
                        connectTask = null;
                        waitingOnConnect = false;
                        return;
                    }
                    else
                    {
                        (this.connectMenu.items[6] as TextMenu.Header).Title = "Connection Failed";
                        Logger.Error(Constants.LOG_PREFIX, "Connection Failed.");
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
                connectMenu.Focused)
            {
                if (Input.MenuConfirm.Pressed && connectMenu.Selection == 7)
                {
                    waitingOnConnect = true;
                    connectTask = ArchipelagoManager.Instance.Connect(true);
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
            //TextInput.OnInput += modTextInput_OnInput;

            RefreshConnectionMenu();

            if (from is OuiModOptionString && pendingSelection != -1)
                this.connectMenu.Selection = pendingSelection;
            pendingSelection = -1;

            this.Visible = true;
            this.connectMenu.Visible = true;
            this.connectMenu.Focused = true;

            yield return null;
        }

        public override IEnumerator Leave(Oui next)
        {
            //TextInput.OnInput -= modTextInput_OnInput;

            yield return Everest.SaveSettings();

            this.Visible = false;
            if (this.connectMenu != null)
            {
                this.connectMenu.Visible = false;
                this.connectMenu.Focused = false;
                Scene.Remove(this.connectMenu);
                this.connectMenu = null;
            }
        }

        private class DimmableButton : TextMenu.Button
        {
            public float DimAlpha = 1f;

            public DimmableButton(string label) : base(label) { }

            public override void Render(Vector2 position, bool highlighted)
            {
                float alpha = Container.Alpha;
                Color color = (highlighted ? Container.HighlightColor : Color.White) * (alpha * DimAlpha);
                Color stroke = Color.Black * (alpha * alpha * alpha * DimAlpha);

                bool twoColumn = Container.InnerContent == TextMenu.InnerContentMode.TwoColumn && !AlwaysCenter;
                Vector2 pos = position + (twoColumn ? Vector2.Zero : new Vector2(Container.Width * 0.5f, 0f));
                Vector2 justify = twoColumn ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);

                ActiveFont.DrawOutline(Label, pos, justify, Vector2.One, color, 2f, stroke);
            }
        }

        public TextMenu CreateConnectMenu()
        {
            Type settings = typeof(CelesteArchipelagoModule);

            TextMenu retMenu = new TextMenu();
            retMenu.BatchMode = true;
            retMenu.CompactWidthMode = true;

            bool noPassword = string.IsNullOrEmpty(CelesteArchipelagoModule.Settings.Password);

            retMenu.Add(new TextMenu.Header("Address"));
            retMenu.Add(new TextMenu.Button(CelesteArchipelagoModule.Settings.Address).Pressed(() => EditField(1, CelesteArchipelagoModule.Settings.Address, 100, 0, v => CelesteArchipelagoModule.Settings.Address = v)));
            retMenu.Add(new TextMenu.Header("Player Name"));
            retMenu.Add(new TextMenu.Button(CelesteArchipelagoModule.Settings.PlayerName).Pressed(() => EditField(3, CelesteArchipelagoModule.Settings.PlayerName, 50, 0, v => CelesteArchipelagoModule.Settings.PlayerName = v)));
            retMenu.Add(new TextMenu.Header("Password"));
            retMenu.Add(new DimmableButton(noPassword ? "No Password" : CelesteArchipelagoModule.Settings.Password)
            {
                DimAlpha = noPassword ? 0.5f : 1f
            }.Pressed(() => EditField(5, CelesteArchipelagoModule.Settings.Password, 50, 0, v => CelesteArchipelagoModule.Settings.Password = v)));
            retMenu.Add(new TextMenu.Header(""));

            TextMenu.Button connectButton = new TextMenu.Button("Connect");
            retMenu.Add(connectButton);

            return retMenu;
        }

        private void EditField(int index, string current, int maxLength, int minLength, Action<string> onChange)
        {
            pendingSelection = index;
            Audio.Play("event:/ui/main/savefile_rename_start");
            Overworld.Goto<OuiModOptionString>().Init<OuiConnection>(current, onChange, maxLength, minLength);
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
                    if (levelSet.Name.StartsWith("StrawberryJam2021"))
                    {
                        foreach (AreaStats area in levelSet.Areas)
                        {
                            StrawberryJam2021Module.SaveData.FilledJamJarSIDs.Add(area.SID);
                        }
                    }
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
                }

                ArchipelagoUtils.setUnlockedAreas();

                modJournal.InitializeMapLookup();

                ArchipelagoManager.Instance.SaveReady = true;

                ArchipelagoManager.Instance.updateUncollectedLocationsInLogic();
            }

            (Scene as Overworld).Goto<OuiHelper_ChapterSelect_LevelSet>().Direction = -1;
        }

                //private void modTextInput_OnInput(char obj)
        //{
        //    if (this.connectMenu.Selection != 7)
        //    {
        //        string currentButtonText = (this.connectMenu.items[this.connectMenu.Selection] as TextMenu.Button).Label;
        //        if (obj == 8 && currentButtonText.Length > 0)
        //        {
        //            (this.connectMenu.items[this.connectMenu.Selection] as TextMenu.Button).Label = currentButtonText.Remove(currentButtonText.Length - 1);
        //        }
        //        else if (obj >= 32 && obj <= 126)
        //        {
        //            (this.connectMenu.items[this.connectMenu.Selection] as TextMenu.Button).Label += obj;
        //        }
        //        else if (obj == 13)
        //        {
        //            this.connectMenu.Selection += 2;
        //        }

        //        Core.CoreModule.Settings.DebugConsole.Keys.Remove(Microsoft.Xna.Framework.Input.Keys.OemPeriod);
        //    }

        //    CelesteArchipelagoModule.Settings.Address = (this.connectMenu.items[1] as TextMenu.Button).Label;
        //    CelesteArchipelagoModule.Settings.PlayerName = (this.connectMenu.items[3] as TextMenu.Button).Label;
        //    CelesteArchipelagoModule.Settings.Password = (this.connectMenu.items[5] as TextMenu.Button).Label;
        //}
    }
}
