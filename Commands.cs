using UnityEngine;
using System.Linq;

namespace FantabulousDebugger;

public class Commands : MonoBehaviour
{
    private static GameObject player;

    public static void ExecuteCommand(string command)
    {
        string[] commandParts = command.Split(' ');
        string commandName = commandParts[0];
        string[] commandArgs = commandParts.Skip(1).ToArray();
        
        switch (commandName.ToLower())
        {
            case "help":
                HelpSystem.HandleHelpCommand(commandParts);
                break;
            case "godmode":
                GameStateCommands.HandleGodmodeCommand();
                break;
            case "tp":
                TeleportSystem.HandleTeleportCommand(commandArgs);
                break;
            case "level":
                LevelSystem.HandleLevelCommand(commandArgs);
                break;
            case "noclip":
                PlayerDebugMovement.HandleNoclipCommand(commandArgs);
                break;
            case "scan":
                ObjectScanner.PrintGameObjects(commandArgs.Length > 0 && commandArgs[0].ToLower() == "detailed" ? false : true);
                break;
            case "inspect":
                ObjectInspector.HandleInspectCommand(commandArgs);
                break;
            case "fullbright":
                VisualEffects.HandleFullbrightCommand(commandArgs);
                break;
            case "maxhealth":
                GameStateCommands.HandleMaxHealthCommand(commandArgs);
                break;
            case "weapons":
                GameStateCommands.HandleWeaponsCommand();
                break;
            case "machinegun":
                GameStateCommands.HandleMachineGunCommand(commandArgs);
                break;
            case "sausage":
                GameStateCommands.HandleAddSausageCommand(commandArgs);
                break;
            case "destroy":
                GameStateCommands.HandleDestroyCommand(commandArgs);
                break;
            case "wireframe":
                VisualEffects.HandleWireframeCommand();
                break;
            case "colliders":
                VisualEffects.HandleCollidersCommand();
                break;
            default:
                FantabulousDebugger.Logger.LogWarning($"Unrecognized command: {command}");
                break;
        }
    }


    private void OnLevelWasLoaded(int level)
    {
        // Reset debug states during any level transition
        PlayerDebugMovement.DisableOnLevelLoad();
        VisualEffects.Reset();
    }

    private void Update()
    {
        if (player == null)
        {
            player = GameObject.Find("Player");
            // Initialize subsystems with player reference
            TeleportSystem.Initialize(player);
            ObjectInspector.Initialize(player);
            PlayerDebugMovement.Initialize(player);
            GameStateCommands.Initialize(player);
        }
        else
        {
            GameStateCommands.ApplyGodmode();

            if (PlayerDebugMovement.Noclip)
            {
                PlayerDebugMovement.HandleNoclipMovement();
            }
        }

        GameStateCommands.Update();
    }

    void OnRenderObject()
    {
        if (VisualEffects.DrawColliders)
        {
            // TODO: Implement colliders drawing
        }
    }
}
