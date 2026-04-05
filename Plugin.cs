using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;

namespace FantabulousDebugger;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class FantabulousDebugger : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    private static bool debugMenuCreated = false;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }
    
    private void Update()
    {
        // Plugin update logic
        if (Input.GetKeyDown(KeyCode.F7))
        {
            string objectName = "Player";
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
            DebugConsoleExtension.CreateDeveloperConsole();
        }
    }
}