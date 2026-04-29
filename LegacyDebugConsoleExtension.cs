using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

namespace FantabulousDebugger;

public static class LegacyDebugConsoleExtension
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
            FantabulousDebugger.Logger.LogError("Player not found for legacy console");
            return;
        }
        if (mainGUI == null)
        {
            FantabulousDebugger.Logger.LogError("MainGUI not found for legacy console");
            return;
        }

        console.player = player;
        console.MainGUI = mainGUI;
        
        FantabulousDebugger.Logger.LogInfo("Legacy console created");
    }

    [HarmonyPatch(typeof(DeveloperConsole), "Update")]
    public class DeveloperConsoleTranspilerPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (var index = 0; index < codes.Count; index++)
            {
                var operation = codes[index];
                
                if (operation.opcode == OpCodes.Ldstr)
                {
                    // Replace old command names with easier ones
                    if (operation.operand?.ToString() == "tpmetoherenow")
                    {
                        codes[index].operand = "tp";
                    }

                    if (operation.operand?.ToString() == "givemegodmodeplease")
                    {
                        codes[index].operand = "godmode";
                    }

                    if (operation.operand?.ToString() == "jumparoundlikebugsbunny")
                    {
                        codes[index].operand = "megajumps";
                    }
                }
            }

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(DeveloperConsole), "MegaJumps")]
    public class DeveloperConsoleMegajumpsFixPatch
    {
        public static void Prefix(DeveloperConsole __instance)
        {
            __instance.visible = false;
            __instance.currText = string.Empty;
        }
    }
}
