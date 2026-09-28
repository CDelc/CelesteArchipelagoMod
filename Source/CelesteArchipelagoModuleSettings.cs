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
    public bool ServerMessages { get; set; } = true;
    
    [SettingIgnore]
    public bool Debug { get; set;  } = false;

    public bool DisplayRoomName { get; set; } = true;
}