using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;


namespace Celeste.Mod.CelesteArchipelago;

public class CelesteArchipelagoModule : EverestModule {
    public static CelesteArchipelagoModule Instance { get; private set; }

    public override Type SettingsType => typeof(CelesteArchipelagoModuleSettings);
    public static CelesteArchipelagoModuleSettings Settings => (CelesteArchipelagoModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(CelesteArchipelagoModuleSession);
    public static CelesteArchipelagoModuleSession Session => (CelesteArchipelagoModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(CelesteArchipelagoModuleSaveData);
    public static CelesteArchipelagoModuleSaveData SaveData => (CelesteArchipelagoModuleSaveData) Instance._SaveData;

    public static bool IsInArchipelagoSave =>
        global::Celeste.SaveData.Instance != null && global::Celeste.SaveData.Instance.FileSlot == Constants.SAVE_ID;

    public CelesteArchipelagoModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(CelesteArchipelagoModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(CelesteArchipelagoModule), LogLevel.Info);
#endif
    }


    public override void Load() {

        new ArchipelagoManager(Celeste.Instance);

        foreach (var item in Constants.modifications)
        {
            item.Load();
            Log("CelesteArchipelago Mod Loaded");
        }
    }


    public override void Unload() {
        foreach (var item in Constants.modifications)
        {
            item.Unload();
        }
    }

    public static void Log(string message)
    {
        Logger.Log(LogLevel.Info, "CelesteArchipelago", message);
    }
}