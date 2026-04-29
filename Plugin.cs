using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace FantabulousDebugger;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class FantabulousDebugger : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    private static bool debugMenuCreated = false;
    
    public static ConfigEntry<bool> LegacyConsoleEnabled;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Application.RegisterLogCallback(HandleLog);
        var bepinexLogListener = new BepInExLogListener(HandleBepInExLog);
        BepInEx.Logging.Logger.Listeners.Add(bepinexLogListener);

        // Initialize config entries
        LegacyConsoleEnabled = Config.Bind("General", "Legacy Console Enabled", false, "Enable or disable the legacy debug console");

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }
    
    private void Update()
    {
        // Plugin update logic
        if (Input.GetKeyDown(KeyCode.F7))
        {
            string objectName = "Main Camera";
            Component[] results = ObjectScanner.FindObjectComponents(objectName);
            if (results.Length > 0)
            {
                Logger.LogInfo($"{objectName} components found: {results.Length} total");
                foreach (var result in results)
                {
                    Logger.LogInfo(result?.GetType().Name ?? $"{objectName} Not found");
                }
            }
            else
            {
                Logger.LogInfo($"{objectName} Not found");
            }
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                ObjectScanner.PrintGameObjects(false);
            }
            else
            {
                ObjectScanner.PrintGameObjects(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Logger.LogInfo($"Level number: {Application.loadedLevel} | Map Name: {Application.loadedLevelName}");
        }

        if (!debugMenuCreated && Time.time > 1f)
        {
            CreateDebugMenu();
            debugMenuCreated = true;
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
        logHistory.Add($"[{type}] {logString}");
        if (logHistory.Count > 200) logHistory.RemoveAt(0);
    }
    
    void HandleBepInExLog(string message) {
        logHistory.Add($"{message}");
        if (logHistory.Count > 200) logHistory.RemoveAt(0);
    }

    public static List<string> logHistory = new();

    private void CreateDebugMenu()
    {
        Logger.LogInfo("Creating debug menu");
        GameObject debugLevelSelectObject = new GameObject("DebugLevelSelectObject");
        debugLoadLevel debugLoadLevelComponent = debugLevelSelectObject.AddComponent<debugLoadLevel>();
        
        debugLoadLevelComponent.player = new GameObject("Player");
        debugLoadLevelComponent.levelToLoad = "hub";
        debugLoadLevelComponent.devbuild = "0.9";
        debugLoadLevelComponent.objectives = "placeholder";
    }

    public class BepInExLogListener : ILogListener
    {
        private System.Action<string> onLogReceived;

        public BepInExLogListener(System.Action<string> callback)
        {
            onLogReceived = callback;
        }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            // Format: [ModName] Message
            string formattedMsg = $"[{eventArgs.Source.SourceName}] {eventArgs.Data}";
            onLogReceived?.Invoke(formattedMsg);
        }

        public void Dispose() { }
    }
    
    [HarmonyPatch(typeof(title), "OnGUI")]
    public static class TitleOnGUIPatch
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerMovement), "Start")]
    public static class PlayerStartPatch
    {
        public static void Postfix()
        {
            if (LegacyConsoleEnabled.Value)
            {
                LegacyDebugConsoleExtension.CreateDeveloperConsole();
            }
            else
            {
                NewDebugConsole.CreateDeveloperConsole();
            }
        }
    }
}