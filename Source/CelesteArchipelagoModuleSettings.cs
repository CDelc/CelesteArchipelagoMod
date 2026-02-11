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
}