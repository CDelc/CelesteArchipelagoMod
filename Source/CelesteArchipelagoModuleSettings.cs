namespace Celeste.Mod.CelesteArchipelago;

public class CelesteArchipelagoModuleSettings : EverestModuleSettings {

    [SettingIgnore]
    public string Address { get; set; } = "archipelago.gg";
    [SettingIgnore]
    [SettingMinLength(1)]
    [SettingMaxLength(16)]
    public string PlayerName { get; set; } = "Player";
    [SettingIgnore]
    public string Password { get; set; } = "";


    #region Send/Receive Messages
    public bool ServerMessages { get; set; } = true;
    public bool RoomPopups { get; set; } = true;

    [SettingRange(1, 10)]
    public int TextBoxDisplayDuration { get; set; } = 3;

    public bool TextBoxQueueSpeedup { get; set; } = true;
    #endregion
}