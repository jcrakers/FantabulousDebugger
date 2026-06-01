using UnityEngine;
using System;

namespace FantabulousDebugger;

public static class LevelSystem
{
    public static void HandleLevelCommand(string[] commandArgs)
    {
        LoadLevel(commandArgs);
    }

    private static void LoadLevel(string[] commandArgs)
    {
        if (commandArgs.Length == 0)
        {
            FantabulousDebugger.Logger.LogWarning("Please provide a level name or index");
            return;
        }
        
        string[] levelNames = new string[] { "title", "intro", "hub", "circlefriendfightyplace", "ShamrockFakeCastle", "signvilleussr", "signmanBattle", "thefantabula", "hubdarkened", "prison", "Sham5KFight1", "ShamrockCastle", "ShamrockKingBoss" };
        
        // Try to parse as integer index first
        if (int.TryParse(commandArgs[0], out int levelIndex))
        {
            if (levelIndex < 0 || levelIndex >= levelNames.Length)
            {
                FantabulousDebugger.Logger.LogWarning($"Please provide an index within the range of 0-{levelNames.Length - 1}");
                return;
            }
            
            Application.LoadLevel(levelNames[levelIndex]);
            NewDebugConsole.mainGUI.paused = false;
            Screen.lockCursor = !NewDebugConsole.mainGUI.paused;
            FantabulousDebugger.Logger.LogInfo($"Loading level {levelNames[levelIndex]} (index {levelIndex})");
            return;
        }
        else
        {
            // Try to find by string name (case-insensitive)
            string levelName = commandArgs[0];
            int foundIndex = Array.FindIndex(levelNames, name => 
                string.Equals(name, levelName, StringComparison.OrdinalIgnoreCase));
            
            if (foundIndex >= 0)
            {
                Application.LoadLevel(levelNames[foundIndex]);
                NewDebugConsole.mainGUI.paused = false;
                Screen.lockCursor = !NewDebugConsole.mainGUI.paused;
                FantabulousDebugger.Logger.LogInfo($"Loading level {levelNames[foundIndex]} (index {foundIndex})");
                return;
            }
            else
            {
                FantabulousDebugger.Logger.LogWarning($"Unknown level: {levelName}. Available levels: {string.Join(", ", levelNames)}");
                return;
            }
        }
    }
}
