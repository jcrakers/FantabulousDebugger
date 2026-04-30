using UnityEngine;
using System.Linq;
using System;

namespace FantabulousDebugger;

public class Commands : MonoBehaviour
{
    private static bool godmode = false;
    private static bool noclip = false;
    private static Vector3 desiredPlayerPosition;
    private static Collider playerCollider;
    private static Rigidbody playerRigidbody;
    private static GameObject player;

    public static void ExecuteCommand(string command)
    {
        string[] commandParts = command.Split(' ');
        string commandName = commandParts[0];
        string[] commandArgs = commandParts.Skip(1).ToArray();
        
        switch (commandName.ToLower())
        {
            case "help":
                HandleHelpCommand(commandParts);
                break;
            case "godmode":
                HandleGodmodeCommand();
                break;
            case "tp":
                HandleTeleportCommand(commandArgs);
                break;
            case "level":
                HandleLevelCommand(commandArgs);
                break;
            case "noclip":
                HandleNoclipCommand();
                break;
            case "scan":
                HandleScanCommand(commandArgs);
                break;
            default:
                FantabulousDebugger.Logger.LogWarning($"Unrecognized command: {command}");
                break;
        }
    }

    private static void HandleHelpCommand(string[] commandParts)
    {
        if (commandParts.Length > 1)
        {
            FantabulousDebugger.Logger.LogInfo(HelpForCommand(commandParts[1]));
            return;
        }

        FantabulousDebugger.Logger.LogInfo(@"Available commands:
help: Tells you about available commands. Pass the command name as an argument to get more info about it.
Godmode: Toggles godmode.
Noclip: Toggles noclip.
TP: Teleports to the coordinates.
Level: Loads the level provided.
Scan: Scans and prints game objects. Use 'scan simple' for basic output.");
    }

    private static void HandleGodmodeCommand()
    {
        godmode = !godmode;
        FantabulousDebugger.Logger.LogInfo($"Godmode: {godmode}");
    }

    private static void HandleTeleportCommand(string[] commandArgs)
    {
        if (commandArgs.Length < 3)
        {
            FantabulousDebugger.Logger.LogWarning("Please provide X, Y, and Z coordinates");
            return;
        }
        
        float x, y, z;
        if (!float.TryParse(commandArgs[0], out x) || !float.TryParse(commandArgs[1], out y) || !float.TryParse(commandArgs[2], out z))
        {
            FantabulousDebugger.Logger.LogWarning("Please provide valid numeric coordinates");
            return;
        }

        player.transform.position = new Vector3(x, y, z);
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        FantabulousDebugger.Logger.LogInfo($"Teleporting to {x}, {y}, {z}");
    }

    private static void HandleLevelCommand(string[] commandArgs)
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

    private static void HandleNoclipCommand()
    {
        ToggleNoclip();
    }

    private static void HandleScanCommand(string[] commandArgs)
    {
        bool detailed = false;
        
        // Check for "detailed" argument to enable detailed output
        if (commandArgs.Length > 0 && commandArgs[0].ToLower() == "detailed")
        {
            detailed = true;
        }
        
        ObjectScanner.PrintGameObjects(detailed);
    }

    private static void ToggleNoclip()
    {
        noclip = !noclip;
        
        if (noclip)
        {
            // Store current position and disable collision
            desiredPlayerPosition = player.transform.position;
            playerCollider = player.GetComponent<Collider>();
            playerRigidbody = player.GetComponent<Rigidbody>();
            
            if (playerCollider != null)
                playerCollider.enabled = false;
            
            if (playerRigidbody != null)
            {
                playerRigidbody.useGravity = false;
                playerRigidbody.velocity = Vector3.zero;
            }
            
            FantabulousDebugger.Logger.LogInfo("Noclip enabled - Use WASD + Space/Shift to move");
        }
        else
        {
            // Re-enable collision
            if (playerCollider != null)
                playerCollider.enabled = true;
            
            if (playerRigidbody != null)
                playerRigidbody.useGravity = true;
            
            FantabulousDebugger.Logger.LogInfo("Noclip disabled");
        }
    }

    private static string HelpForCommand(string command)
    {
        return $"Help for {command}";
    }

    private static float noclipSpeed = 50f;
    private static float noclipBoostSpeed = 100f;

    private void Update()
    {
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (godmode)
        {
            Stats stats = player.GetComponent<Stats>();
            stats.health = stats.maxHealth;
        }

        if (noclip)
        {
            HandleNoclipMovement();
        }      
    }

    private void HandleNoclipMovement()
    {
        // Get camera direction for movement
        Vector3 moveDirection = Vector3.zero;
        Camera playerCamera = Camera.main;
        
        if (playerCamera != null)
        {
            // Get forward and right vectors relative to camera
            Vector3 forward = playerCamera.transform.forward;
            forward.Normalize();
            
            Vector3 right = playerCamera.transform.right;
            right.Normalize();
            
            // Movement controls based on camera direction
            if (Input.GetKey(KeyCode.W))
            {
                moveDirection += forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveDirection -= forward;
            }
            if (Input.GetKey(KeyCode.A))
            {
                moveDirection -= right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveDirection += right;
            }
        }
        else
        {
            // Fallback to transform-based movement if no camera
            if (Input.GetKey(KeyCode.W))
            {
                moveDirection += transform.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveDirection -= transform.forward;
            }
            if (Input.GetKey(KeyCode.A))
            {
                moveDirection -= transform.right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveDirection += transform.right;
            }
        }
        
        // Vertical movement
        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection += Vector3.up;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveDirection -= Vector3.up;
        }

        // Normalize movement direction and apply speed
        if (moveDirection != Vector3.zero)
        {
            moveDirection = moveDirection.normalized;
            desiredPlayerPosition += moveDirection * noclipSpeed * Time.deltaTime;
        }

        // Boost speed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            noclipSpeed = noclipBoostSpeed;
        }
        else
        {
            noclipSpeed = 50f;
        }

        // Halt player momentum and update position
        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
        
        player.transform.position = desiredPlayerPosition;
    }
}
