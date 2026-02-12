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

    public HashSet<long> CrystalHeartsVanilla { get; set; } = new HashSet<long>();

    public HashSet<long> CrystalHeartsCollab { get; set; } = new HashSet<long>();

    public HashSet<long> SilverBerriesUnlocked { get; set; } = new HashSet<long>();

    public HashSet<long> SummitGemsUnlocked { get; set; } = new HashSet<long>();


    public HashSet<long> LocationsChecked { get; set; } = new HashSet<long>();

}