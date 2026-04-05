using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

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

    /*[HarmonyPatch(typeof(DeveloperConsole), "Update")]
    public class DeveloperConsoleTranspilerPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (var index = 0; index < codes.Count; index++)
            {
                var operation = codes[index];
                
                if (operation.opcode == OpCodes.Ldfld)
                {
                    // FantabulousDebugger.Logger.LogInfo($"Ldfld operation at index {index}: {operation.operand?.ToString()}");
                    if (operation.operand?.ToString() == "System.Boolean godMode")
                    {
                        codes[index] = new CodeInstruction(OpCodes.Pop);
                        codes[index].operand = null;
                        codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldc_I4_0));
                    }

                    if (operation.operand?.ToString() == "System.Boolean megaJumps")
                    {
                        codes[index] = new CodeInstruction(OpCodes.Pop);
                        codes[index].operand = null;
                        codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldc_I4_0));
                    }
                }
            }

            return codes.AsEnumerable();
        }
    }*/

    [HarmonyPatch(typeof(DeveloperConsole), "Update")]
    public class DeveloperConsoleBetterMegajumpsGodmodePatch
    {
        public static void Prefix(DeveloperConsole __instance)
        {
            if (__instance.player == null)
            {
                __instance.player = GameObject.Find("Player");
                return;
            }
        }

        public static void Postfix(DeveloperConsole __instance)
        {
            
        }
    }
}
