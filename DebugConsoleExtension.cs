using UnityEngine;

namespace FantabulousDebugger;

public static class DebugConsoleExtension
{
    public static void CreateDeveloperConsole()
    {
        GameObject consoleObject = new GameObject("DeveloperConsole");
        DeveloperConsole console = consoleObject.AddComponent<DeveloperConsole>();
        GameObject.DontDestroyOnLoad(consoleObject);
        
        GameObject player = GameObject.Find("Player");
        MainGUI mainGUI = Object.FindObjectOfType<MainGUI>();

        if (player == null)
        {
            FantabulousDebugger.Logger.LogError("Player not found for developer console");
            return;
        }
        if (mainGUI == null)
        {
            FantabulousDebugger.Logger.LogError("MainGUI not found for developer console");
            return;
        }

        console.player = player;
        console.MainGUI = mainGUI;
        
        FantabulousDebugger.Logger.LogInfo("Developer console created");
    }
}