using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using static Celeste.Mod.CelesteArchipelago.ArchipelagoData.ArchipelagoMapper;

namespace Celeste.Mod.CelesteArchipelago;

public class CelesteArchipelagoModuleSettings : EverestModuleSettings
{

    [SettingIgnore]
    public string Address { get; set; } = "archipelago.gg";
    [SettingIgnore]
    [SettingMinLength(1)]
    [SettingMaxLength(16)]
    public string PlayerName { get; set; } = "Player";
    [SettingIgnore]
    public string Password { get; set; } = "";
    public bool ServerMessages { get; set; } = true;

    public bool DisplayRoomName { get; set; } = true;

    public bool ShowLevelWaypoints { get; set; } = true;

    public bool HideProgressionHighlighting { get; set; } = false;


//#if DEBUG
//    [SettingIgnore]
//    public DebugMechanicSettings DebugMechanics { get; set; } = new DebugMechanicSettings();

//    public bool DebugMode { get; set; } = false;

//    public class DebugMechanicSettings
//    {
//        private const int ConsolidatedStart = 1000;

//#if DEBUG
//        public Dictionary<string, bool> Overrides { get; set; } = new Dictionary<string, bool>();

//        public bool IsEnabled(Mechanic mechanic)
//        {
//            if ((int)mechanic >= ConsolidatedStart)
//                return true;
//            return !Overrides.TryGetValue(mechanic.ToString(), out bool enabled) || enabled;
//        }

//        public void BuildMenu(TextMenu menu)
//        {
//            var sub = new TextMenuExt.SubMenu("Debug: Mechanics", false);

//            // Use names rather than GetValues so aliased enum values can't collapse or duplicate entries.
//            foreach (string name in Enum.GetNames(typeof(Mechanic)))
//            {
//                Mechanic mechanic = (Mechanic)Enum.Parse(typeof(Mechanic), name);
//                if ((int)mechanic >= ConsolidatedStart)
//                    continue;

//                string key = name;
//                bool current = !Overrides.TryGetValue(key, out bool stored) || stored;

//                sub.Add(new TextMenu.OnOff(PrettyName(name), current).Change(value =>
//                {
//                    if (value)
//                        Overrides.Remove(key);   // default is on, don't store it
//                    else
//                        Overrides[key] = false;
//                }));
//            }

//            menu.Add(sub);
//        }

//        // DASH_CRYSTALS -> "Dash Crystals"
//        private static string PrettyName(string enumName)
//        {
//            string[] words = enumName.ToLowerInvariant().Split('_');
//            for (int i = 0; i < words.Length; i++)
//                if (words[i].Length > 0)
//                    words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1);
//            return string.Join(" ", words);
//        }
//    }
//#endif
}