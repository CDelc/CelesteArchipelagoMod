using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.CelesteArchipelago.Constants.Constants;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.CelesteArchipelago.Archipelago
{
    public class ArchipelagoManager : DrawableGameComponent
    {

        public struct ArchipelagoMessage
        {
            public enum MessageType
            {
                General,
                Chat,
                Server,
                ItemReceive,
                ItemSend,
                ItemHint,
                Literature
            }

            public string Text { get; init; } = "";
            public MessageType Type { get; set; } = MessageType.General;
            public ItemFlags Flags { get; set; } = ItemFlags.Advancement;
            public bool Strawberry { get; set; } = false;

            public ArchipelagoMessage(string text, MessageType type = MessageType.General, ItemFlags flags = ItemFlags.Advancement, bool strawberry = false)
            {
                Text = text;
                Type = type;
                Flags = flags;
                Strawberry = strawberry;
            }
        }


        private static readonly Version _supportedArchipelagoVersion = new(0, 6, 5);

        #region SlotData
        public int start_level_set = 0;
        public bool include_beginner = false;
        public bool include_intermediate = false;
        public bool include_advanced = false;
        public bool include_expert = false;
        public bool include_grandmaster = false;
        public bool include_cracked_grandmaster = false;
        public bool include_b_sides = false;
        public bool include_c_sides = false;
        public bool include_farewell = false;

        public bool randomize_climb = false;
        public bool randomize_checkpoints = false;
        public bool room_checks = false;
        public bool include_heart_side_golden = false;
        public bool include_beginner_silvers = false;
        public bool include_intermediate_silvers = false;
        public bool include_advanced_silvers = false;
        public bool include_expert_silvers = false;
        public bool include_grandmaster_silvers = false;
        public bool include_cracked_grandmaster_silvers = false;
        public bool include_a_sides_goldens = false;
        public bool include_b_sides_goldens = false;
        public bool include_c_sides_goldens = false;
        public bool include_farewell_golden = false;
        public int win_condition_level = 0;
        public bool protect_victory_level_checkpoints = false;
        public int strawberries_required_percentage = 80;
        public int total_strawberries = 100;
        public int required_strawberries = 0;
        public bool require_moon_berry = false;
        public bool lock_win_condition_behind_strawberries = false;
        #endregion

        private ArchipelagoSession _session;

        public static ArchipelagoManager Instance { get; private set; }
        
        public bool Ready { get; private set; }
        public bool WasConnected { get; private set; }
        public int Slot => _session.ConnectionInfo.Slot;

        public bool GoalSent = false;

        public List<Tuple<int, ItemInfo>> ItemQueue { get; private set; } = new();
        public List<long> CollectedLocations { get; private set; } = new();
        public HashSet<long> SentLocations { get; set; } = [];
        public List<ArchipelagoMessage> MessageLog { get; set; } = new();

        public int ServerItemsRcv = -1;
        private bool ItemRcvCallbackSet = false;


        public static readonly HashSet<string> PermanentUnlockLevels = new HashSet<string>
        {
            "StrawberryJam2021/0-Lobbies/1-Beginner",
            "StrawberryJam2021/0-Lobbies/2-Intermediate",
            "StrawberryJam2021/0-Lobbies/3-Advanced",
            "StrawberryJam2021/0-Lobbies/4-Expert",
            "StrawberryJam2021/0-Lobbies/5-Grandmaster",
        };


        public ArchipelagoManager(Game game) : base(game)
        {
            game.Components.Add(this);
            Instance = this;
        }


        public override void Update(GameTime gameTime)
        {
            if (Ready)
            {
                try
                {
                    CheckReceivedItemQueue();
                    HandleCollectedLocations();
                    CheckLocationsToSend();
                }
                catch (ArchipelagoSocketClosedException)
                {
                    Disconnect();
                }
            }
        }


        public async Task<LoginFailure> Connect()
        {
            _session = ArchipelagoSessionFactory.CreateSession(CelesteArchipelagoModule.Settings.Address);

            Ready = false;
            ItemQueue = new();
            GoalSent = false;

            try
            {
                await _session.ConnectAsync();
            }
            catch (Exception ex)
            {
                Disconnect();
                string message = $"Failed to connect to Archipelago Server at {CelesteArchipelagoModule.Settings.Address} : {ex.Message}";
                Monocle.Engine.Commands.Log(message, Color.Red);
                return new(message);
            }

            _session.Socket.ErrorReceived += OnError;
            _session.Socket.SocketClosed += OnSocketClosed;
            _session.MessageLog.OnMessageReceived += OnMessageReceived;
            _session.Items.ItemReceived += OnItemReceived;
            _session.Locations.CheckedLocationsUpdated += OnLocationReceived;

            var result = await _session.LoginAsync(
                "Celeste Modded",
                CelesteArchipelagoModule.Settings.PlayerName,
                ItemsHandlingFlags.AllItems,
                _supportedArchipelagoVersion,
                null,
                null,
                CelesteArchipelagoModule.Settings.Password);


            if (!result.Successful)
            {
                Disconnect();
                Monocle.Engine.Commands.Log((result as LoginFailure).ToString(), Color.Red);
                return result as LoginFailure;
            }

            LoginSuccessful loginData = (LoginSuccessful)result;

            object value;

            start_level_set = Convert.ToInt32(loginData.SlotData.TryGetValue("start_level_set", out value) ? value : 0);
            include_beginner = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_beginner", out value) ? value : false);
            include_intermediate = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_intermediate", out value) ? value : false);
            include_advanced = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_advanced", out value) ? value : false);
            include_expert = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_expert", out value) ? value : false);
            include_grandmaster = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_grandmaster", out value) ? value : false);
            include_cracked_grandmaster = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_cracked_grandmaster", out value) ? value : false);
            include_b_sides = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_b_sides", out value) ? value : false);
            include_c_sides = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_c_sides", out value) ? value : false);
            include_farewell = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_farewell", out value) ? value : false);

            randomize_climb = Convert.ToBoolean(loginData.SlotData.TryGetValue("randomize_climb", out value) ? value : false);
            randomize_checkpoints = Convert.ToBoolean(loginData.SlotData.TryGetValue("randomize_checkpoints", out value) ? value : false);
            room_checks = Convert.ToBoolean(loginData.SlotData.TryGetValue("room_checks", out value) ? value : false);

            include_heart_side_golden = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_heart_side_golden", out value) ? value : false);
            include_beginner_silvers = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_beginner_silvers", out value) ? value : false);
            include_intermediate_silvers = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_intermediate_silvers", out value) ? value : false);
            include_advanced_silvers = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_advanced_silvers", out value) ? value : false);
            include_expert_silvers = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_expert_silvers", out value) ? value : false);
            include_grandmaster_silvers = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_grandmaster_silvers", out value) ? value : false);
            include_cracked_grandmaster_silvers = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_cracked_grandmaster_silvers", out value) ? value : false);
            include_a_sides_goldens = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_a_sides_goldens", out value) ? value : false);
            include_b_sides_goldens = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_b_sides_goldens", out value) ? value : false);
            include_c_sides_goldens = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_c_sides_goldens", out value) ? value : false);
            include_farewell_golden = Convert.ToBoolean(loginData.SlotData.TryGetValue("include_farewell_golden", out value) ? value : false);
            win_condition_level = Convert.ToInt32(loginData.SlotData.TryGetValue("win_condition_level", out value) ? value : 0);
            protect_victory_level_checkpoints = Convert.ToBoolean(loginData.SlotData.TryGetValue("protect_victory_level_checkpoints", out value) ? value : false);
            strawberries_required_percentage = Convert.ToInt32(loginData.SlotData.TryGetValue("strawberries_required_percentage", out value) ? value : 0);
            total_strawberries = Convert.ToInt32(loginData.SlotData.TryGetValue("total_strawberries", out value) ? value : 0);
            required_strawberries = Convert.ToInt32(loginData.SlotData.TryGetValue("required_strawberries", out value) ? value : 0);
            require_moon_berry = Convert.ToBoolean(loginData.SlotData.TryGetValue("require_moon_berry", out value) ? value : false);
            lock_win_condition_behind_strawberries = Convert.ToBoolean(loginData.SlotData.TryGetValue("lock_win_condition_behind_strawberries", out value) ? value : false);

            this.AddItemsRcvCallback($"Celeste_Open_Rcv_{_session.Players.GetPlayerName(this.Slot)}", ItemsRcvUpdated);
            this.ServerItemsRcv = -1;

            Ready = true;
            WasConnected = true;
            
            return null;
        }

        public async Task<LoginFailure> Disconnect(bool attemptReconnect = true)
        {
            this.ServerItemsRcv = -1;
            this.ItemQueue.Clear();
            this.MessageLog.Clear();
            this.GoalSent = false;

            if (!attemptReconnect)
            {
                this.WasConnected = false;
            }

            if (_session != null)
            {
                _session.Socket.ErrorReceived -= OnError;
                _session.Socket.SocketClosed -= OnSocketClosed;
                _session.Items.ItemReceived -= OnItemReceived;
                _session.Locations.CheckedLocationsUpdated -= OnLocationReceived;
                _session.MessageLog.OnMessageReceived -= OnMessageReceived;
                _session.Socket.DisconnectAsync();
                _session = null;
            }

            if (this.WasConnected && attemptReconnect)
            {
                return await this.Connect();
            }
            else
            {
                return null;
            }
        }

        private void OnMessageReceived(LogMessage message)
        {
            switch (message)
            {
                case HintItemSendLogMessage:
                    HintItemSendLogMessage hintItemSendMessage = (HintItemSendLogMessage)message;

                    if (hintItemSendMessage.IsRelatedToActivePlayer)
                    {
                        if (!hintItemSendMessage.IsFound)
                        {
                            var item = hintItemSendMessage.Item;

                            string itemColor = GetColorString(item.Flags);
                            string sendPlayerColor = (hintItemSendMessage.Sender == this.Slot) ? "#EE00EE" : "#FAFAD2";
                            string recvPlayerColor = (hintItemSendMessage.Receiver == this.Slot) ? "#EE00EE" : "#FAFAD2";
                            string prettyMessage = $"{{{recvPlayerColor}}}{hintItemSendMessage.Receiver.Name}{{#}}'s {{{itemColor}}}{item.ItemName}{{#}} is at {{#00FF7F}}{hintItemSendMessage.Item.LocationName}{{#}} in {{{sendPlayerColor}}}{hintItemSendMessage.Sender.Name}{{#}}'s World.";

                            MessageLog.Add(new ArchipelagoMessage(prettyMessage, ArchipelagoMessage.MessageType.ItemHint));
                        }
                        Logger.Log("AP", message.ToString());
                        Monocle.Engine.Commands.Log(message.ToString(), Color.Orange);
                    }
                    break;
                case ItemSendLogMessage:
                    ItemSendLogMessage itemSendMessage = (ItemSendLogMessage)message;

                    if (itemSendMessage.IsRelatedToActivePlayer && !itemSendMessage.IsReceiverTheActivePlayer)
                    {
                        string itemColor = GetColorString(itemSendMessage.Item.Flags);
                        string prettyMessage = $"Sent {{{itemColor}}}{itemSendMessage.Item.ItemName}{{#}} to {{#FAFAD2}}{itemSendMessage.Receiver.Name}{{#}}.";

                        MessageLog.Add(new ArchipelagoMessage(prettyMessage.ToString(), ArchipelagoMessage.MessageType.ItemSend, itemSendMessage.Item.Flags));
                        Logger.Log(LOG_PREFIX, message.ToString());
                        Monocle.Engine.Commands.Log(message.ToString(), Color.Lime);
                    }
                    break;
                case CommandResultLogMessage:
                case ServerChatLogMessage:
                case CountdownLogMessage:
                    Monocle.Engine.Commands.Log(message.ToString());
                    MessageLog.Add(new ArchipelagoMessage(message.ToString(), ArchipelagoMessage.MessageType.Server));
                    break;
                case ChatLogMessage:
                    Monocle.Engine.Commands.Log(message.ToString());
                    MessageLog.Add(new ArchipelagoMessage(message.ToString(), ArchipelagoMessage.MessageType.Chat));
                    break;
                case GoalLogMessage:
                    Monocle.Engine.Commands.Log(message.ToString(), Color.Gold);
                    MessageLog.Add(new ArchipelagoMessage(message.ToString()));
                    break;
            }
        }

        private void OnLocationReceived(ReadOnlyCollection<long> newCheckedLocations)
        {
            foreach (var newLoc in newCheckedLocations)
            {
                CollectedLocations.Add(newLoc);
            }
        }

        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            var i = helper.Index;
            while (helper.Any())
            {
                ItemQueue.Add(new(i++, helper.DequeueItem()));
            }
        }

        private void OnSocketClosed(string reason)
        {
            Logger.Error(LOG_PREFIX, reason);
            Monocle.Engine.Commands.Log(reason, Color.Red);

            ArchipelagoManager.Instance.Disconnect();
        }

        private void OnError(Exception e, string message)
        {
            Logger.Error(LOG_PREFIX, message);
            Monocle.Engine.Commands.Log(message, Color.Red);

            ArchipelagoManager.Instance.Disconnect();
        }

        private static string GetColorString(ItemFlags flags)
        {
            string itemColor = "";
            if ((flags & ItemFlags.Advancement) != 0)
            {
                itemColor = "#AF99EF";
            }
            else if ((flags & ItemFlags.NeverExclude) != 0)
            {
                itemColor = "#6D8BE8";
            }
            else if ((flags & ItemFlags.Trap) != 0)
            {
                itemColor = "#FA8072";
            }
            else
            {
                itemColor = "#00EEEE";
            }

            return itemColor;
        }

        public void CheckReceivedItemQueue()
        {
            if (this.Slot == -1 || SaveData.Instance == null || CelesteArchipelagoModule.SaveData == null)
            {
                return;
            }

            if (this.ServerItemsRcv < 0)
            {
                this.ServerItemsRcv = GetRemoteItemsRcv();
                return;
            }

            SaveData.Instance.TotalStrawberries_Safe = CelesteArchipelagoModule.SaveData.Strawberries;

            for (int index = CelesteArchipelagoModule.SaveData.ItemRcv; index < ItemQueue.Count; index++)
            {
                var item = ItemQueue[index].Item2;

                string receivedMessage = $"Received {item.ItemDisplayName} from {GetPlayerName(item.Player)}.";
                string itemColor = GetColorString(item.Flags);
                string prettyMessage = "";

                if (item.Player == this.Slot)
                {
                    prettyMessage = $"You found your {{{itemColor}}}{item.ItemDisplayName}{{#}}.";
                }
                else
                {
                    prettyMessage = $"Received {{{itemColor}}}{item.ItemDisplayName}{{#}} from {{#FAFAD2}}{GetPlayerName(item.Player)}{{#}}.";
                }

                if ((item.ItemId < 0xCA10020 || item.ItemId >= 0xCA10050) && index >= this.ServerItemsRcv)
                {
                    Logger.Info("AP", receivedMessage);
                    MessageLog.Add(new ArchipelagoMessage(prettyMessage, ArchipelagoMessage.MessageType.ItemReceive, item.Flags));
                    Monocle.Engine.Commands.Log(receivedMessage, Color.DeepPink);
                }

                switch (item.ItemId)
                {
                    //Mechanic
                    case long id when id >= 200000000000 && id < 300000000000:
                        {
                            CelesteArchipelagoModule.SaveData.Mechanics[id] = true;
                            break;
                        }
                    //Checkpoint
                    case long id when id >= 300000000000 && id < 400000000000:
                        {
                            CelesteArchipelagoModule.SaveData.UnlockedCheckpoints.Add(id);
                            break;
                        }
                    //Level
                    case long id when id >= 400000000000 && id < 500000000000:
                        {
                            CelesteArchipelagoModule.SaveData.LevelUnlocks.Add(ArchipelagoMapper.ArchipelagoIDToSID(id));
                            break;
                        }
                    //Key
                    case long id when id >= 500000000000 && id < 600000000000:
                        {
                            CelesteArchipelagoModule.SaveData.UnlockedKeys.Add(id);
                            break;
                        }
                    //Vanilla Crystal Heart
                    case long id when id >= 600000000000 && id < 700000000000:
                        {
                            CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Add(id);
                            break;
                        }
                    //Collab Crystal Heart
                    case long id when id >= 700000000000 && id < 800000000000:
                        {
                            CelesteArchipelagoModule.SaveData.CrystalHeartsCollab.Add(id);
                            break;
                        }
                    //Strawberry
                    case 800000000000:
                        {
                            CelesteArchipelagoModule.SaveData.Strawberries++;
                            break;
                        }
                    //Moon Berry
                    case 900000000000:
                        {
                            CelesteArchipelagoModule.SaveData.Strawberries++;
                            break;
                        }
                    //Silver berries
                    case long id when id >= 1000000000000 && id < 1100000000000:
                        {
                            CelesteArchipelagoModule.SaveData.SilverBerriesUnlocked.Add(id);
                            break;
                        }
                    //Filler Items
                    case long id when id >= 1100000000000 && id < 1200000000000:
                        {
                            break;
                        }
                }

                CelesteArchipelagoModule.SaveData.ItemRcv = index + 1;
            }

            if (CelesteArchipelagoModule.SaveData.ItemRcv > this.ServerItemsRcv)
            {
                this.ServerItemsRcv = CelesteArchipelagoModule.SaveData.ItemRcv;
                SetRemoteItemsRcv(CelesteArchipelagoModule.SaveData.ItemRcv);
            }
        }

        public void CheckLocationsToSend()
        {
            if (SaveData.Instance == null || CelesteArchipelagoModule.SaveData == null)
            {
                return;
            }

            List<long> locationsToCheck = new List<long>();
            foreach (long locationID in CelesteArchipelagoModule.SaveData.LocationsChecked)
            {
                if (!SentLocations.Contains(locationID))
                {
                    locationsToCheck.Add(locationID);
                }
            }
            
            CheckLocations(locationsToCheck.ToArray());
        }

        public void HandleCollectedLocations()
        {
            if (SaveData.Instance == null || CelesteArchipelagoModule.SaveData == null)
            {
                return;
            }

            foreach (long newLoc in CollectedLocations)
            {
                CelesteArchipelagoModule.SaveData.LocationsChecked.Add(newLoc);
                //strawberry
                if(newLoc >= 200000000000 && newLoc < 300000000000)
                {
                    EntityID strawberry = ArchipelagoMapper.getStrawberryEntityID(newLoc);
                    long levelID = ArchipelagoMapper.extractLevelID(newLoc);
                    AreaModeStats areaModeStats = ArchipelagoMapper.getAreaModeStats(levelID);

                    if (areaModeStats.Strawberries.Add(strawberry))
                    {
                        areaModeStats.TotalStrawberries++;
                    }
                }
                //cassette
                else if (newLoc >= 300000000000 && newLoc < 400000000000)
                {
                    long levelID = ArchipelagoMapper.extractLevelID(newLoc);
                    (string SID, AreaMode mode) = ArchipelagoMapper.getSID(levelID);
                    AreaData areaData = AreaData.Get(SID);
                    if (areaData == null)
                    {
                        throw new ApplicationException($"Areadata not found for SID {SID}");
                    }
                    AreaKey areaKey = areaData.ToKey(mode);
                    SaveData.Instance.RegisterCassette(areaKey);
                }
                //level_clear
                else if (newLoc >= 400000000000 && newLoc < 500000000000)
                {

                }
                //level clear mini heart
                else if (newLoc >= 500000000000 && newLoc < 600000000000)
                {
                    (string SID, AreaMode mode) = ArchipelagoMapper.getSID(ArchipelagoMapper.extractLevelID(newLoc));
                    AreaData areaData = AreaData.Get(SID);
                    if (areaData == null)
                    {
                        throw new ApplicationException($"Areadata not found for SID {SID}");
                    }
                    SaveData.Instance.Areas_Safe[areaData.ID].Modes[(int)mode].HeartGem = true;
                }
                //crystal heart
                else if (newLoc >= 600000000000 && newLoc < 700000000000)
                {
                    (string SID, AreaMode mode) = ArchipelagoMapper.getSID(ArchipelagoMapper.extractLevelID(newLoc));
                    AreaData areaData = AreaData.Get(SID);
                    if (areaData == null)
                    {
                        throw new ApplicationException($"Areadata not found for SID {SID}");
                    }
                    SaveData.Instance.Areas_Safe[areaData.ID].Modes[(int)mode].HeartGem = true;
                }
                //checkpoint
                else if (newLoc >= 700000000000 && newLoc < 800000000000)
                {

                }
                //key
                else if (newLoc >= 800000000000 && newLoc < 900000000000)
                {

                }
                //golden berry
                else if (newLoc >= 900000000000 && newLoc < 1000000000000)
                {
                    EntityID strawberry = ArchipelagoMapper.getStrawberryEntityID(newLoc);
                    long levelID = ArchipelagoMapper.extractLevelID(newLoc);
                    AreaModeStats areaModeStats = ArchipelagoMapper.getAreaModeStats(levelID);

                    if (areaModeStats.Strawberries.Add(strawberry))
                    {
                        areaModeStats.TotalStrawberries++;
                    }
                }
                //silver_berry
                else if (newLoc >= 1100000000000 && newLoc < 1200000000000)
                {

                }
                //rainbow_berry
                else if (newLoc >= 1200000000000 && newLoc < 1300000000000)
                {

                }
                //winged_golden
                else if (newLoc >= 1300000000000 && newLoc < 1400000000000)
                {
                    EntityID strawberry = ArchipelagoMapper.getStrawberryEntityID(newLoc);
                    long levelID = ArchipelagoMapper.extractLevelID(newLoc);
                    AreaModeStats areaModeStats = ArchipelagoMapper.getAreaModeStats(levelID);

                    if (areaModeStats.Strawberries.Add(strawberry))
                    {
                        areaModeStats.TotalStrawberries++;
                    }
                }
                //room
                else if (newLoc >= 1400000000000 && newLoc < 1500000000000)
                {

                }
                //gem
                else if (newLoc >= 1500000000000 && newLoc < 1600000000000)
                {

                }
            }

            CollectedLocations.Clear();
        }

        public void CheckLocations(long[] locations)
        {
            foreach (var locationID in locations)
            {
                SentLocations.Add(locationID);
            }

            try
            {
                _session.Locations.CompleteLocationChecksAsync(locations);
            }
            catch (ArchipelagoSocketClosedException)
            {
                Disconnect();
            }
        }

        public void UpdateGameStatus(ArchipelagoClientState state)
        {
            try
            {
                if (state == ArchipelagoClientState.ClientGoal)
                {
                    if (GoalSent)
                    {
                        return;
                    }
                    GoalSent = true;
                }
                SendPacket(new StatusUpdatePacket { Status = state });
            }
            catch (ArchipelagoSocketClosedException)
            {
                Disconnect();
            }
        }

        private void SendPacket(ArchipelagoPacketBase packet)
        {
            try
            {
                _session.Socket.SendPacket(packet);
            }
            catch (ArchipelagoSocketClosedException)
            {
                Disconnect();
            }
        }

        public string GetPlayerName(int slot)
        {
            if (slot == 0)
            {
                return "Archipelago";
            }

            var name = _session.Players.GetPlayerAlias(slot);
            return string.IsNullOrEmpty(name) ? $"Unknown Player {slot}" : name;
        }

        public int GetRemoteItemsRcv()
        {
            return this.GetInt($"Celeste_Modded_Rcv_{_session.Players.GetPlayerName(this.Slot)}");
        }

        public void SetRemoteItemsRcv(int value)
        {
            this.Set($"Celeste_Modded_Rcv_{_session.Players.GetPlayerName(this.Slot)}", value);
        }

        private int GetInt(string key)
        {
            try
            {
                if (!_session.DataStorage[key])
                {
                    return 0;
                }

                return _session.DataStorage[key];
            }
            catch (ArchipelagoSocketClosedException)
            {
                Disconnect();
            }

            return 0;
        }

        private void Set(string key, int value)
        {
            try
            {
                var token = JToken.FromObject(value);
                _session.DataStorage[key] = token;
            }
            catch (ArchipelagoSocketClosedException)
            {
                Disconnect();
            }
        }

        private void Set(string key, string value)
        {
            try
            {
                var token = JToken.FromObject(value);
                _session.DataStorage[key] = token;
            }
            catch (ArchipelagoSocketClosedException)
            {
                Disconnect();
            }
        }


        public void AddItemsRcvCallback(string key, Action<int> callback)
        {
            if (!ItemRcvCallbackSet)
            {
                ItemRcvCallbackSet = true;
                _session.DataStorage[key].OnValueChanged += (oldData, newData, _) => {
                    int newItemsRcv = JsonConvert.DeserializeObject<int>(newData.ToString());
                    callback(newItemsRcv);
                };
            }
        }

        public void ItemsRcvUpdated(int newItemsRcv)
        {
            this.ServerItemsRcv = newItemsRcv;
        }
    }
}
