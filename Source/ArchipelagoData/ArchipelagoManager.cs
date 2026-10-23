using Archipelago.MultiClient.Net;
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using static Celeste.Mod.CelesteArchipelago.Constants;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.CelesteArchipelago.ArchipelagoData
{
    public class ArchipelagoManager : DrawableGameComponent
    {

        public struct ArchipelagoMessage
        {
            public enum MessageType
            {
                General,
                Server,
                ItemReceive,
                ItemSend,
                ItemHint
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
        public bool randomize_checkpoints = false;
        public bool winged_golden = false;

        public int win_condition_level_selection = 0;
        public (string sid, AreaMode mode) win_condition_level = ("Celeste/7-Summit", AreaMode.Normal);
        public bool protect_victory_level_checkpoints = false;
        public int required_strawberries = 0;
        public bool require_moon_berry = false;
        public bool require_berries_for_goal = true;
        public bool open_heart_gates = false;
        public int precollected_items = 0;
        public bool exclude_multiroom_berries = false;


        public string apworld_version = "";
        public string minimum_mod_version = "";

        public HashSet<long> included_levels = new HashSet<long>();
        public HashSet<long> preunlocked_levels = new HashSet<long>();
        public HashSet<long> deathless_levels = new HashSet<long>();
        public HashSet<long> roomcheck_levels = new HashSet<long>();
        public HashSet<long> autocheck_locations = new HashSet<long>();
        public Dictionary<string, long> itemNameToId = new Dictionary<string, long>();
        public Dictionary<long, List<long>> allLocationByLevelID = new Dictionary<long, List<long>>();
        public Dictionary<long, List<List<long>>> logical_summary = new Dictionary<long, List<List<long>>>();
        public Dictionary<long, Dictionary<long, string>> room_names = new Dictionary<long, Dictionary<long, string>>();
        public Dictionary<int, List<int>> item_mapping = new Dictionary<int, List<int>>();
        #endregion

        private ArchipelagoSession _session;

        public static ArchipelagoManager Instance { get; private set; }

        public bool SaveReady = false;

        public bool Ready { get; private set; }
        public bool Disconnecting { get; private set; }
        public bool VersionError { get; private set; }
        public bool WasConnected { get; private set; }
        public int Slot => _session.ConnectionInfo.Slot;

        public bool GoalSent = false;

        public List<Tuple<int, ItemInfo>> ItemQueue { get; private set; } = new();
        public ConcurrentQueue<long> CollectedLocations { get; private set; } = new();
        public HashSet<long> SentLocations { get; set; } = [];
        public ConcurrentQueue<ArchipelagoMessage> MessageQueue { get; } = new();

        public int ServerItemsRcv = -1;
        private bool ItemRcvCallbackSet = false;

        private static readonly HashSet<String> ALL_POSSIBLE_LEVEL_SETS = new HashSet<string> {
            "Celeste",
            "StrawberryJam2021/0-Lobbies",
        };

        public HashSet<String> included_level_sets = new HashSet<string>();

        public ArchipelagoManager(Game game) : base(game)
        {
            game.Components.Add(this);
            Instance = this;
        }


        public override void Update(GameTime gameTime)
        {
            if (Ready && SaveReady)
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


        public async Task<LoginFailure> Connect(bool freshSave = false)
        {
            if (Disconnecting)
            {
                return new("Disconnect operation still running, please wait.");
            }
            Ready = false;
            SaveReady = false;
            ItemQueue = new();
            GoalSent = false;
            VersionError = false;

            if (_session != null) { await Disconnect(attemptReconnect: false); }
            CollectedLocations.Clear();
            ItemRcvCallbackSet = false;

            try
            {
                _session = ArchipelagoSessionFactory.CreateSession(CelesteArchipelagoModule.Settings.Address);
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
                CelesteArchipelagoModule.Settings.Password == "" ? null : CelesteArchipelagoModule.Settings.Password);


            if (!result.Successful)
            {
                Disconnect();
                Monocle.Engine.Commands.Log((result as LoginFailure).ToString(), Color.Red);
                return result as LoginFailure;
            }

            LoginSuccessful loginData = (LoginSuccessful)result;

            object value;

            randomize_checkpoints = Convert.ToBoolean(loginData.SlotData.TryGetValue("randomize_checkpoints", out value) ? value : false);
            winged_golden = Convert.ToBoolean(loginData.SlotData.TryGetValue("winged_golden", out value) ? value : false);
            exclude_multiroom_berries = Convert.ToBoolean(loginData.SlotData.TryGetValue("exclude_multiroom_berries", out value) ? value : 0);

            win_condition_level_selection = Convert.ToInt32(loginData.SlotData.TryGetValue("win_condition_level", out value) ? value : 0);
            win_condition_level = ArchipelagoMapper.getWinConditionLevel(win_condition_level_selection);
            protect_victory_level_checkpoints = Convert.ToBoolean(loginData.SlotData.TryGetValue("protect_victory_level_checkpoints", out value) ? value : false);
            required_strawberries = Convert.ToInt32(loginData.SlotData.TryGetValue("required_strawberries", out value) ? value : 0);
            require_moon_berry = Convert.ToBoolean(loginData.SlotData.TryGetValue("require_moon_berry", out value) ? value : false);
            require_berries_for_goal = Convert.ToBoolean(loginData.SlotData.TryGetValue("require_berries_for_goal", out value) ? value : false);
            open_heart_gates = Convert.ToBoolean(loginData.SlotData.TryGetValue("open_heart_gates", out value) ? value : false);
            precollected_items = Convert.ToInt32(loginData.SlotData.TryGetValue("precollected_items", out value) ? value : 0);

            apworld_version = Convert.ToString(loginData.SlotData.TryGetValue("apworld_version", out value) ? value : "");
            minimum_mod_version = Convert.ToString(loginData.SlotData.TryGetValue("minimum_mod_version", out value) ? value : "");

            if(loginData.SlotData.TryGetValue("enabled_level_list", out object enabledList) && enabledList is JArray arr)
            {
                included_levels = arr.ToObject<HashSet<long>>();
            }

            if (loginData.SlotData.TryGetValue("start_unlocked_level_list", out object unlockedList) && unlockedList is JArray arr_2)
            {
                preunlocked_levels = arr_2.ToObject<HashSet<long>>();
            }

            if (loginData.SlotData.TryGetValue("deathless_level_list", out object deathlessList) && deathlessList is JArray arr_3)
            {
                deathless_levels = arr_3.ToObject<HashSet<long>>();
            }

            if (loginData.SlotData.TryGetValue("roomcheck_level_list", out object roomcheckList) && roomcheckList is JArray arr_4)
            {
                roomcheck_levels = arr_4.ToObject<HashSet<long>>();
            }

            if (loginData.SlotData.TryGetValue("autocheck_locations", out object autocheckList) && autocheckList is JArray arr_5)
            {
                autocheck_locations = arr_5.ToObject<HashSet<long>>();
            }

            if (loginData.SlotData.TryGetValue("all_locations_by_level", out object locationMap) && locationMap is JObject obj_2)
            {
                allLocationByLevelID = obj_2.ToObject<Dictionary<long, List<long>>>();
            }

            if (loginData.SlotData.TryGetValue("logical_summary", out object logicalSummary) && logicalSummary is JObject obj_3)
            {
                logical_summary = obj_3.ToObject<Dictionary<long, List<List<long>>>>();
            }

            if (loginData.SlotData.TryGetValue("room_names", out object roomNames) && roomNames is JObject obj_4)
            {
                room_names = obj_4.ToObject<Dictionary<long, Dictionary<long, string>>>();
            }

            if (loginData.SlotData.TryGetValue("item_mapping", out object itemMapping) && itemMapping is JObject obj_5)
            {
                item_mapping = obj_5.ToObject<Dictionary<int, List<int>>>();
            }

            ArchipelagoMapper.buildRoomMap();

            int versionCompatible;
            try
            {
                versionCompatible = VersionCompatible();
            } catch(Exception e)
            {
                Disconnect();
                string message = $"Error while parsing world versions, let the developer know this happened.";
                CelesteArchipelagoModule.Error(message);
                return new(message);
            }

            if (versionCompatible != 0)
            {
                Disconnect();
                string message = $"Version mismatch/error: Mod version {CelesteArchipelagoModule.MOD_VERSION} is not compatible with APWorld Version {apworld_version}. " +
                    (versionCompatible == 1 ?  $"Please update mod to at least {minimum_mod_version}" : $"Please update APWorld to at least {CelesteArchipelagoModule.APWORLD_MINIMUM_VERSION}");
                CelesteArchipelagoModule.Error(message);
                VersionError = true;
                return new(message);
            }

            this.AddItemsRcvCallback($"Celeste_Modded_Rcv_{_session.Players.GetPlayerName(this.Slot)}", ItemsRcvUpdated);
            this.ServerItemsRcv = -1;

            

            updateCassetteAndCrystalHeartItemTextMapping();
            foreach(string set in ALL_POSSIBLE_LEVEL_SETS)
            {
                string parent_set = set;
                if (set.StartsWith("StrawberryJam2021"))
                {
                    parent_set = "StrawberryJam2021";
                }
                if(ArchipelagoUtils.anyLevelIncludedInSet(parent_set))
                {
                    included_level_sets.Add(set);
                }
            }

            WasConnected = true;
            Ready = true;

            if(!freshSave)
            {
                SaveReady = true;
            }

            return null;
        }

        
        public async Task<LoginFailure> Disconnect(bool attemptReconnect = true)
        {
            Ready = false;
            Disconnecting = true;
            this.ServerItemsRcv = -1;
            this.ItemQueue.Clear();
            this.MessageQueue.Clear();
            this.GoalSent = false;
            this.SentLocations.Clear();
            this.CollectedLocations.Clear();

            if (!attemptReconnect)
            {
                this.WasConnected = false;
            }

            try
            {
                if (_session != null)
                {
                    _session.Socket.ErrorReceived -= OnError;
                    _session.Socket.SocketClosed -= OnSocketClosed;
                    _session.Items.ItemReceived -= OnItemReceived;
                    _session.Locations.CheckedLocationsUpdated -= OnLocationReceived;
                    _session.MessageLog.OnMessageReceived -= OnMessageReceived;

                    if (_session.Socket != null)
                    {
                        await _session.Socket.DisconnectAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Constants.LOG_PREFIX, $"Socket disconnect threw an exception: {ex.Message}");
            }
            finally
            {
                _session = null;
                Disconnecting = false;
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

        // We populate this data from the AP server once connected. Caching used for displaying item names on Cassette and Crystal Heart screens
        private void updateCassetteAndCrystalHeartItemTextMapping()
        {
            ArchipelagoMapper.cassette_crystal_heart_display_text.Clear();
            List<long> locationIDs = new List<long>();
            foreach((string sid, AreaMode mode) in ArchipelagoMapper.getAllSIDs())
            {
                locationIDs.Add(ArchipelagoMapper.getCassetteLocationID(sid, mode));
                locationIDs.Add(ArchipelagoMapper.getCrystalHeartLocationID(sid, mode));
                locationIDs.Add(ArchipelagoMapper.getLevelCompleteLocationID(sid, mode));
            }

            _session.Locations.ScoutLocationsAsync(HintCreationPolicy.None, locationIDs.ToArray()).ContinueWith((Task<Dictionary<long, ScoutedItemInfo>> responseTask) =>
            {
                if (responseTask.IsFaulted || responseTask.IsCanceled) return;

                Dictionary<long, ScoutedItemInfo> response = responseTask.Result;

                foreach(KeyValuePair<long, ScoutedItemInfo> entry in response)
                {
                    ArchipelagoMapper.cassette_crystal_heart_display_text.Add(entry.Key, entry.Value.ItemName);
                }
            });
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

                            string prettyMessage = $"{hintItemSendMessage.Receiver.Name}'s {item.ItemName} is at {hintItemSendMessage.Item.LocationName} in {hintItemSendMessage.Sender.Name}'s World";

                            MessageQueue.Enqueue(new ArchipelagoMessage(prettyMessage, ArchipelagoMessage.MessageType.ItemHint));
                        }
                        Logger.Log("AP", message.ToString());
                        Monocle.Engine.Commands.Log(message.ToString(), Color.Orange);
                    }
                    break;
                case ItemSendLogMessage:
                    ItemSendLogMessage itemSendMessage = (ItemSendLogMessage)message;

                    if (itemSendMessage.IsRelatedToActivePlayer && !itemSendMessage.IsReceiverTheActivePlayer)
                    {
                        string prettyMessage = $"Sent {itemSendMessage.Item.ItemName} to {itemSendMessage.Receiver.Name}";

                        MessageQueue.Enqueue(new ArchipelagoMessage(prettyMessage, ArchipelagoMessage.MessageType.ItemSend, itemSendMessage.Item.Flags));
                        Logger.Log(LOG_PREFIX, message.ToString());
                        Monocle.Engine.Commands.Log(message.ToString(), Color.Lime);
                    }
                    break;
                case CommandResultLogMessage:
                case ServerChatLogMessage:
                case CountdownLogMessage:
                    Monocle.Engine.Commands.Log(message.ToString());
                    MessageQueue.Enqueue(new ArchipelagoMessage(message.ToString(), ArchipelagoMessage.MessageType.Server));
                    break;
                case GoalLogMessage:
                    Monocle.Engine.Commands.Log(message.ToString(), Color.Gold);
                    MessageQueue.Enqueue(new ArchipelagoMessage(message.ToString()));
                    break;
            }
        }

        private void OnLocationReceived(ReadOnlyCollection<long> newCheckedLocations)
        {
            foreach (var newLoc in newCheckedLocations)
            {
                CollectedLocations.Enqueue(newLoc);
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

            bool updateInternalLogic = false;
            for (int index = CelesteArchipelagoModule.SaveData.ItemRcv; index < ItemQueue.Count; index++)
            {
                updateInternalLogic = true;
                var item = ItemQueue[index].Item2;

                if (index >= precollected_items)
                {

                    string receivedMessage = $"Received {item.ItemDisplayName} from {GetPlayerName(item.Player)}";
                    string prettyMessage = "";

                    if (item.Player == this.Slot)
                    {
                        prettyMessage = $"You found your {item.ItemDisplayName}";
                    }
                    else
                    {
                        prettyMessage = $"Received {item.ItemDisplayName} from {GetPlayerName(item.Player)}";
                    }

                    if (index >= this.ServerItemsRcv)
                    {
                        Logger.Info("CelesteArchipelagoMod", receivedMessage);
                        MessageQueue.Enqueue(new ArchipelagoMessage(prettyMessage, ArchipelagoMessage.MessageType.ItemReceive, item.Flags));
                        Monocle.Engine.Commands.Log(receivedMessage, Color.DeepPink);
                    }
                }

                CelesteArchipelagoModule.SaveData.UniqueItemsCollected.Add(item.ItemId);
                switch (item.ItemId)
                {
                    case 100000000000:
                        {
                            CelesteArchipelagoModule.SaveData.GoalLevelCleared = true;
                            break;
                        }
                    //Mechanic
                    case long id when id >= 200000000000 && id < 300000000000:
                        {
                            CelesteArchipelagoModule.SaveData.Mechanics.Add(id);
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
                    //Key Door
                    case long id when id >= 500000000000 && id < 600000000000:
                        {
                            CelesteArchipelagoModule.SaveData.UnlockedKeyDoors.Add(id);
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
                            CelesteArchipelagoModule.SaveData.moonBerryCollected = true;
                            break;
                        }
                    ////Silver berries
                    //case long id when id >= 1000000000000 && id < 1100000000000:
                    //    {
                    //        CelesteArchipelagoModule.SaveData.SilverBerriesUnlocked.Add(id);
                    //        break;
                    //    }
                    //Summit Gems
                    case long id when id >= 1100000000000 && id < 1200000000000:
                        {
                            CelesteArchipelagoModule.SaveData.SummitGemsUnlocked.Add(id);
                            break;
                        }
                    //Filler Items
                    case long id when id >= 1200000000000 && id < 1300000000000:
                        {
                            break;
                        }
                }

                CelesteArchipelagoModule.SaveData.ItemRcv = index + 1;
            }

            if (updateInternalLogic)
            {
                updateUncollectedLocationsInLogic();
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

            bool updateInternalLogic = false;
            while (CollectedLocations.TryDequeue(out long newLoc))
            {
                updateInternalLogic = true;
                CelesteArchipelagoModule.SaveData.LocationsChecked.Add(newLoc);
                //strawberry
                if (newLoc >= 200000000000 && newLoc < 300000000000)
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
                else if (newLoc >= 1000000000000 && newLoc < 1100000000000)
                {
                    EntityID strawberry = ArchipelagoMapper.getStrawberryEntityID(newLoc);
                    long levelID = ArchipelagoMapper.extractLevelID(newLoc);
                    AreaModeStats areaModeStats = ArchipelagoMapper.getAreaModeStats(levelID);

                    if (areaModeStats.Strawberries.Add(strawberry))
                    {
                        areaModeStats.TotalStrawberries++;
                    }
                }
                ////rainbow_berry
                //else if (newLoc >= 1100000000000 && newLoc < 1200000000000)
                //{

                //}
                //winged_golden
                else if (newLoc >= 1200000000000 && newLoc < 1300000000000)
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
                else if (newLoc >= 1300000000000 && newLoc < 1400000000000)
                {

                }
                //gem
                else if (newLoc >= 1400000000000 && newLoc < 1500000000000)
                {
                    SaveData.Instance.RegisterSummitGem(ArchipelagoMapper.summitGemIndexMapping[ArchipelagoMapper.extractMetadata(newLoc)]);
                }
            }
            if (updateInternalLogic)
            {
                updateUncollectedLocationsInLogic();
            }

            CollectedLocations.Clear();
        }

        public void CheckLocations(long[] locations)
        {
            if(locations.Length == 0) return;

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

        public void CheckCompleteGame()
        {
            CelesteArchipelagoModuleSaveData save = CelesteArchipelagoModule.SaveData;
            if (save.Strawberries >= required_strawberries &&
                (!require_moon_berry || save.moonBerryCollected) &&
                save.GoalLevelCleared)
            {
                UpdateGameStatus(ArchipelagoClientState.ClientGoal);
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

        private int VersionCompatible()
        {
            bool a = Version.TryParse(apworld_version, out Version apworldVersion);
            bool b = Version.TryParse(minimum_mod_version, out Version modMinimumVersion);
            bool c = Version.TryParse(CelesteArchipelagoModule.MOD_VERSION, out Version modVersion);
            bool d = Version.TryParse(CelesteArchipelagoModule.APWORLD_MINIMUM_VERSION, out Version apworldMinimumVersion);

            if (!a || !b || !c || !d)
            {
                Logger.Warn(LOG_PREFIX, $"Version parsing failed. apworld_version: {apworld_version}, minimum_mod_version: {minimum_mod_version}, mod_version: {CelesteArchipelagoModule.MOD_VERSION}");
                throw new Exception("Version parsing failed.");
            }

            return modVersion < modMinimumVersion ? 1 : apworldVersion < apworldMinimumVersion ? 2 : 0;
        }

        public void updateUncollectedLocationsInLogic()
        {
            HashSet<long> levelsWithProgressCopy = new HashSet<long>();
            foreach (long locationID in logical_summary.Keys)
            {
                if (CelesteArchipelagoModule.SaveData.LocationsChecked.Contains(locationID))
                {
                    CelesteArchipelagoModule.SaveData.UncollectedLocationsInLogic.Remove(locationID);
                }
                else if (ArchipelagoUtils.checkAccessRule(logical_summary[locationID]))
                {
                    CelesteArchipelagoModule.SaveData.UncollectedLocationsInLogic.Add(locationID);
                    levelsWithProgressCopy.Add(ArchipelagoMapper.extractLevelID(locationID));
                }
            }
            CelesteArchipelagoModule.SaveData.LevelsWithPotentialProgress = levelsWithProgressCopy;

            //CelesteArchipelagoModule.Log(string.Join(",", CelesteArchipelagoModule.SaveData.UniqueItemsCollected));
            //CelesteArchipelagoModule.Log(string.Join(",", CelesteArchipelagoModule.SaveData.LocationsChecked));
            //CelesteArchipelagoModule.Log(string.Join(",", CelesteArchipelagoModule.SaveData.UncollectedLocationsInLogic));
            //CelesteArchipelagoModule.Log(string.Join(",", CelesteArchipelagoModule.SaveData.LevelsWithPotentialProgress));


            AutoCheckLocations();
        }

        // For start items and auto-check rooms
        public void AutoCheckLocations()
        {
            foreach (long locationID in autocheck_locations)
            {
                if (CelesteArchipelagoModule.SaveData.UncollectedLocationsInLogic.Contains(locationID))
                {
                    CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);
                }
            }
        }
    }
}
