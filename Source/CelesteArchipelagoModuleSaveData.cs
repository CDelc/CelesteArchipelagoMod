using System.Collections.Generic;

namespace Celeste.Mod.CelesteArchipelago;

public class CelesteArchipelagoModuleSaveData : EverestModuleSaveData
{
    public int ItemRcv { get; set; } = 0;
    public int Strawberries { get; set; } = 0;


    public Dictionary<long, bool> Mechanics { get; set; } = new Dictionary<long, bool>();

    public HashSet<(string SID, AreaMode mode)> LevelUnlocks { get; set; } = new HashSet<(string SID, AreaMode mode)>();

    public HashSet<long> UnlockedCheckpoints { get; set; } = new HashSet<long>();

    public HashSet<long> UnlockedKeys { get; set; } = new HashSet<long>();

    /// <summary>
    /// Vanilla crystal heart items received (600B-700B range).
    /// These count toward vanilla heart gates.
    /// </summary>
    public HashSet<long> CrystalHeartsVanilla { get; set; } = new HashSet<long>();

    /// <summary>
    /// All collab crystal heart items received (700B-800B range).
    /// Kept for general tracking / backward compatibility.
    /// </summary>
    public HashSet<long> CrystalHeartsCollab { get; set; } = new HashSet<long>();

    /// <summary>
    /// Heartside crystal heart items from the collab range that count
    /// toward vanilla heart gates (alongside CrystalHeartsVanilla).
    /// </summary>
    public HashSet<long> CrystalHeartsHeartsides { get; set; } = new HashSet<long>();

    /// <summary>
    /// Collab mini heart items grouped by lobby category (LevelCategory int value as key).
    /// Each lobby's heart gate only counts items in its own bucket.
    /// </summary>
    public Dictionary<int, HashSet<long>> CrystalHeartsByLobby { get; set; } = new Dictionary<int, HashSet<long>>();

    public HashSet<long> SilverBerriesUnlocked { get; set; } = new HashSet<long>();

    public HashSet<long> SummitGemsUnlocked { get; set; } = new HashSet<long>();


    public HashSet<long> LocationsChecked { get; set; } = new HashSet<long>();

}