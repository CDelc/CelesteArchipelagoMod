using Celeste.Mod.CelesteArchipelago.Archipelago;
using Celeste.Mod.CelesteArchipelago.UI;
using Celeste.Mod.CollabUtils2.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.CelesteArchipelago;

public class CelesteArchipelagoModule : EverestModule {
    public static CelesteArchipelagoModule Instance { get; private set; }

    public override Type SettingsType => typeof(CelesteArchipelagoModuleSettings);
    public static CelesteArchipelagoModuleSettings Settings => (CelesteArchipelagoModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(CelesteArchipelagoModuleSession);
    public static CelesteArchipelagoModuleSession Session => (CelesteArchipelagoModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(CelesteArchipelagoModuleSaveData);
    public static CelesteArchipelagoModuleSaveData SaveData => (CelesteArchipelagoModuleSaveData) Instance._SaveData;

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

        foreach (var item in Celeste.Mod.CelesteArchipelago.Constants.modifications)
        {
            item.Load();
            Log("CelesteArchipelago Mod Loaded");
        }
    }


    public override void Unload() {
        foreach (var item in Celeste.Mod.CelesteArchipelago.Constants.modifications)
        {
            item.Unload();
        }
    }

    public static void Log(string message)
    {
        Logger.Log(LogLevel.Info, "CelesteArchipelago", message);
    }
}