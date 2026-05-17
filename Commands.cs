using UnityEngine;
using System.Linq;
using System;

namespace FantabulousDebugger;

public class Commands : MonoBehaviour
{
    private static bool godmode = false;
    private static bool noclip = false;
    private static bool fullbright = false;
    private static Vector3 desiredPlayerPosition;
    private static Collider playerCollider;
    private static Rigidbody playerRigidbody;
    private static GameObject player;
    
    // Fog settings storage
    private static bool originalFogState;
    private static Color originalFogColor;
    private static float originalFogDensity;
    private static float originalFogStartDistance;
    private static float originalFogEndDistance;
    private static bool fogSettingsStored = false;

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
                HandleNoclipCommand(commandArgs);
                break;
            case "scan":
                HandleScanCommand(commandArgs);
                break;
            case "inspect":
                HandleInspectCommand(commandArgs);
                break;
            case "fullbright":
                HandleFullbrightCommand(commandArgs);
                break;
            case "maxhealth":
                HandleMaxHealthCommand(commandArgs);
                break;
            case "weapons":
                HandleWeaponsCommand();
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
Scan: Scans and prints game objects. Use 'scan simple' for basic output.
Inspect: Inspects object you're looking at or provide object name. Use 'inspect object component' for specific component inspection.
Fullbright: Toggles fullbright lighting mode.");
    }

    private static void HandleGodmodeCommand()
    {
        godmode = !godmode;
        FantabulousDebugger.Logger.LogInfo($"Godmode: {godmode}");
    }

    private static void HandleTeleportCommand(string[] commandArgs)
    {
        if (commandArgs.Length == 0)
        {
            FantabulousDebugger.Logger.LogWarning("Please provide coordinates or object name");
            return;
        }
        
        // Check if first argument is an object name (non-numeric)
        if (!float.TryParse(commandArgs[0], out float x))
        {
            // Teleport to object
            TeleportToObject(commandArgs[0]);
            return;
        }
        
        // Must be coordinate teleportation from here
        if (commandArgs.Length < 3)
        {
            FantabulousDebugger.Logger.LogWarning("Please provide X, Y, and Z coordinates");
            return;
        }
        
        float y, z;
        if (!float.TryParse(commandArgs[1], out y) || !float.TryParse(commandArgs[2], out z))
        {
            FantabulousDebugger.Logger.LogWarning("Please provide valid numeric coordinates");
            return;
        }

        Vector3 targetPosition = new Vector3(x, y, z);
        PerformTeleport(targetPosition);
        FantabulousDebugger.Logger.LogInfo($"Teleporting to {x}, {y}, {z}");
    }
    
    private static void PerformTeleport(Vector3 targetPosition)
    {
        // Update player position
        player.transform.position = targetPosition;
        
        // Reset velocity to prevent physics issues
        if (player.GetComponent<Rigidbody>() != null)
        {
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        
        // If noclip is enabled, update desired position as well
        if (noclip)
        {
            desiredPlayerPosition = targetPosition;
        }
    }
    
    private static void TeleportToObject(string objectName)
    {
        // Find object by exact name first
        GameObject targetObject = GameObject.Find(objectName);
        
        if (targetObject == null)
        {
            // Try to find by partial name (case-insensitive)
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains(objectName.ToLower()))
                {
                    targetObject = obj;
                    break;
                }
            }
        }
        
        if (targetObject == null)
        {
            FantabulousDebugger.Logger.LogWarning($"Object '{objectName}' not found.");
            return;
        }
        
        // Teleport player to object position
        Vector3 targetPosition = targetObject.transform.position;
        PerformTeleport(targetPosition);
        
        FantabulousDebugger.Logger.LogInfo($"Teleporting to {targetObject.name} at position {targetPosition}");
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

    private static void HandleNoclipCommand(string[] commandArgs)
    {
        if (commandArgs.Length == 0)
        {
            // Toggle noclip
            ToggleNoclip();
            return;
        }
        
        if (commandArgs[0].ToLower() == "speed")
        {
            // Set noclip speed
            if (commandArgs.Length < 2)
            {
                FantabulousDebugger.Logger.LogWarning("Please provide a speed value. Usage: noclip speed <value>");
                return;
            }
            
            if (float.TryParse(commandArgs[1], out float newSpeed))
            {
                if (newSpeed <= 0)
                {
                    FantabulousDebugger.Logger.LogWarning("Speed must be greater than 0");
                    return;
                }
                
                noclipSpeed = newSpeed;
                FantabulousDebugger.Logger.LogInfo($"Noclip speed set to {noclipSpeed}");
            }
            else
            {
                FantabulousDebugger.Logger.LogWarning("Invalid speed value. Please provide a valid number");
            }
            return;
        }
        
        if (commandArgs[0].ToLower() == "boost")
        {
            // Set noclip boost multiplier
            if (commandArgs.Length < 2)
            {
                FantabulousDebugger.Logger.LogWarning("Please provide a boost multiplier. Usage: noclip boost <multiplier>");
                return;
            }
            
            if (float.TryParse(commandArgs[1], out float newBoost))
            {
                if (newBoost <= 0)
                {
                    FantabulousDebugger.Logger.LogWarning("Boost multiplier must be greater than 0");
                    return;
                }
                
                noclipBoostSpeed = noclipSpeed * newBoost;
                FantabulousDebugger.Logger.LogInfo($"Noclip boost multiplier set to {newBoost} (boost speed: {noclipBoostSpeed})");
            }
            else
            {
                FantabulousDebugger.Logger.LogWarning("Invalid boost value. Please provide a valid number");
            }
            return;
        }
        
        // If we get here, it's an invalid argument
        FantabulousDebugger.Logger.LogWarning("Invalid noclip argument. Usage: noclip [speed <value>|boost <multiplier>]");
    }

    private static void HandleFullbrightCommand(string[] commandArgs)
    {
        if (commandArgs.Length == 0)
        {
            // Toggle fullbright
            ToggleFullbright();
            return;
        }
        
        // Check for on/off arguments
        if (commandArgs[0].ToLower() == "on")
        {
            if (!fullbright)
            {
                ToggleFullbright();
            }
            FantabulousDebugger.Logger.LogInfo("Fullbright enabled");
            return;
        }
        
        if (commandArgs[0].ToLower() == "off")
        {
            if (fullbright)
            {
                ToggleFullbright();
            }
            FantabulousDebugger.Logger.LogInfo("Fullbright disabled");
            return;
        }
        
        FantabulousDebugger.Logger.LogWarning("Invalid fullbright argument. Usage: fullbright [on|off]");
    }

    private static void ToggleFullbright()
    {
        fullbright = !fullbright;
        
        // Find all lights in the scene and adjust them
        Light[] lights = FindObjectsOfType<Light>();
        
        if (fullbright)
        {
            // Store original fog settings before modifying
            if (!fogSettingsStored)
            {
                StoreFogSettings();
            }
            
            // Enable fullbright mode
            foreach (Light light in lights)
            {
                light.intensity = Mathf.Max(light.intensity, 2.0f);
                if (light.type == LightType.Directional)
                {
                    light.intensity = 1.5f;
                }
            }
            
            // Add a bright light if no lights exist
            if (lights.Length == 0)
            {
                GameObject brightLight = new GameObject("FullbrightLight");
                Light newLight = brightLight.AddComponent<Light>();
                newLight.type = LightType.Directional;
                newLight.intensity = 1.5f;
                newLight.color = Color.white;
                brightLight.transform.rotation = Quaternion.Euler(45, 45, 0);
            }
            
            // Remove fog for maximum visibility
            RenderSettings.fog = false;
            
            FantabulousDebugger.Logger.LogInfo("Fullbright enabled - Scene lighting enhanced and fog removed");
        }
        else
        {
            // Restore normal lighting
            foreach (Light light in lights)
            {
                if (light.gameObject.name == "FullbrightLight")
                {
                    DestroyImmediate(light.gameObject);
                }
                else
                {
                    // Reset to reasonable defaults
                    if (light.type == LightType.Directional)
                    {
                        light.intensity = 1.0f;
                    }
                    else
                    {
                        light.intensity = Mathf.Min(light.intensity, 1.0f);
                    }
                }
            }
            
            // Restore original fog settings
            if (fogSettingsStored)
            {
                RestoreFogSettings();
            }
            
            FantabulousDebugger.Logger.LogInfo("Fullbright disabled - Normal lighting and fog restored");
        }
    }
    
    private static void StoreFogSettings()
    {
        originalFogState = RenderSettings.fog;
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogStartDistance = RenderSettings.fogStartDistance;
        originalFogEndDistance = RenderSettings.fogEndDistance;
        fogSettingsStored = true;
    }
    
    private static void RestoreFogSettings()
    {
        RenderSettings.fog = originalFogState;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogStartDistance = originalFogStartDistance;
        RenderSettings.fogEndDistance = originalFogEndDistance;
    }

    private static void HandleScanCommand(string[] commandArgs)
    {
        bool simple = true;
        
        // Check for "detailed" argument to enable detailed output
        if (commandArgs.Length > 0 && commandArgs[0].ToLower() == "detailed")
        {
            simple = false;
        }
        
        ObjectScanner.PrintGameObjects(simple);
    }

    private static void HandleInspectCommand(string[] commandArgs)
    {
        GameObject targetObject = null;
        string componentName = null;
        
        if (commandArgs.Length > 0)
        {
            // Check if we have component inspection (object + component)
            if (commandArgs.Length >= 2)
            {
                // First part is object name, rest is component name
                string objectName = commandArgs[0];
                componentName = string.Join(" ", commandArgs.Skip(1).ToArray());
                
                targetObject = GameObject.Find(objectName);
                
                if (targetObject == null)
                {
                    // Try to find by partial name
                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.ToLower().Contains(objectName.ToLower()))
                        {
                            targetObject = obj;
                            break;
                        }
                    }
                }
                
                if (targetObject == null)
                {
                    FantabulousDebugger.Logger.LogWarning($"Object '{objectName}' not found.");
                    return;
                }
            }
            else
            {
                // Single argument - object name or raycast
                string objectName = commandArgs[0];
                targetObject = GameObject.Find(objectName);
                
                if (targetObject == null)
                {
                    // Try to find by partial name
                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.ToLower().Contains(objectName.ToLower()))
                        {
                            targetObject = obj;
                            break;
                        }
                    }
                }
                
                if (targetObject == null)
                {
                    FantabulousDebugger.Logger.LogWarning($"Object '{objectName}' not found.");
                    return;
                }
            }
        }
        else
        {
            // Raycast to find object player is looking at
            Camera playerCamera = Camera.main;
            if (playerCamera != null && player != null)
            {
                Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, 100f))
                {
                    targetObject = hit.collider.gameObject;
                }
                else
                {
                    FantabulousDebugger.Logger.LogWarning("No object found in front of player.");
                    return;
                }
            }
            else
            {
                FantabulousDebugger.Logger.LogWarning("Cannot perform raycast - no camera or player found.");
                return;
            }
        }
        
        // Display information
        if (componentName != null)
        {
            DisplayComponentInfo(targetObject, componentName);
        }
        else
        {
            DisplayObjectInfo(targetObject);
        }
    }
    
    private static void DisplayObjectInfo(GameObject obj)
    {
        FantabulousDebugger.Logger.LogInfo($"=== Object Information ===");
        FantabulousDebugger.Logger.LogInfo($"Name: {obj.name}");
        FantabulousDebugger.Logger.LogInfo($"Tag: {obj.tag}");
        FantabulousDebugger.Logger.LogInfo($"Layer: {obj.layer}");
        FantabulousDebugger.Logger.LogInfo($"Active: {obj.activeInHierarchy}");
        
        // Position information
        FantabulousDebugger.Logger.LogInfo($"Position: {obj.transform.position}");
        FantabulousDebugger.Logger.LogInfo($"Rotation: {obj.transform.rotation.eulerAngles}");
        FantabulousDebugger.Logger.LogInfo($"Scale: {obj.transform.localScale}");
        
        // Components
        Component[] components = obj.GetComponents<Component>();
        string[] componentNames = components.Select(c => c.GetType().Name).ToArray();
        FantabulousDebugger.Logger.LogInfo($"Components ({components.Length}): {string.Join(", ", componentNames)}");
        
        // Parent/Child information
        if (obj.transform.parent != null)
        {
            FantabulousDebugger.Logger.LogInfo($"Parent: {obj.transform.parent.name}");
        }
        
        if (obj.transform.childCount > 0)
        {
            string[] childNames = new string[obj.transform.childCount];
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                childNames[i] = obj.transform.GetChild(i).name;
            }
            FantabulousDebugger.Logger.LogInfo($"Children: {string.Join(", ", childNames)}");
        }
        
        // Specific component details
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            FantabulousDebugger.Logger.LogInfo($"Rigidbody - Mass: {rb.mass}, Velocity: {rb.velocity}");
        }
        
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            FantabulousDebugger.Logger.LogInfo($"Collider: {collider.GetType().Name} - IsTrigger: {collider.isTrigger}");
        }
        
        FantabulousDebugger.Logger.LogInfo($"=========================");
    }
    
    private static void DisplayComponentInfo(GameObject obj, string componentName)
    {
        Component[] components = obj.GetComponents<Component>();
        Component targetComponent = null;
        
        // Find component by name (case-insensitive)
        foreach (Component comp in components)
        {
            if (comp.GetType().Name.ToLower() == componentName.ToLower())
            {
                targetComponent = comp;
                break;
            }
        }
        
        if (targetComponent == null)
        {
            // Try partial match
            foreach (Component comp in components)
            {
                if (comp.GetType().Name.ToLower().Contains(componentName.ToLower()))
                {
                    targetComponent = comp;
                    break;
                }
            }
        }
        
        if (targetComponent == null)
        {
            string[] componentNames = components.Select(c => c.GetType().Name).ToArray();
            FantabulousDebugger.Logger.LogWarning($"Component '{componentName}' not found on object '{obj.name}'. Available components: {string.Join(", ", componentNames)}");
            return;
        }
        
        // Display component information
        FantabulousDebugger.Logger.LogInfo($"=== Component Information ===");
        FantabulousDebugger.Logger.LogInfo($"Object: {obj.name}");
        FantabulousDebugger.Logger.LogInfo($"Component: {targetComponent.GetType().Name}");
        FantabulousDebugger.Logger.LogInfo($"Enabled: {targetComponent.GetType().GetProperty("enabled")?.GetValue(targetComponent, null) ?? "N/A"}");
        
        // Component-specific information
        if (targetComponent is Transform)
        {
            Transform t = (Transform)targetComponent;
            FantabulousDebugger.Logger.LogInfo($"Position: {t.position}");
            FantabulousDebugger.Logger.LogInfo($"Rotation: {t.rotation.eulerAngles}");
            FantabulousDebugger.Logger.LogInfo($"Scale: {t.localScale}");
        }
        else if (targetComponent is Rigidbody)
        {
            Rigidbody rb = (Rigidbody)targetComponent;
            FantabulousDebugger.Logger.LogInfo($"Mass: {rb.mass}");
            FantabulousDebugger.Logger.LogInfo($"Velocity: {rb.velocity}");
            FantabulousDebugger.Logger.LogInfo($"Angular Velocity: {rb.angularVelocity}");
            FantabulousDebugger.Logger.LogInfo($"Use Gravity: {rb.useGravity}");
            FantabulousDebugger.Logger.LogInfo($"Is Kinematic: {rb.isKinematic}");
        }
        else if (targetComponent is Collider)
        {
            Collider col = (Collider)targetComponent;
            FantabulousDebugger.Logger.LogInfo($"Is Trigger: {col.isTrigger}");
            FantabulousDebugger.Logger.LogInfo($"Bounds: {col.bounds}");
            
            if (col is BoxCollider)
            {
                BoxCollider box = (BoxCollider)col;
                FantabulousDebugger.Logger.LogInfo($"Size: {box.size}");
                FantabulousDebugger.Logger.LogInfo($"Center: {box.center}");
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = (SphereCollider)col;
                FantabulousDebugger.Logger.LogInfo($"Radius: {sphere.radius}");
                FantabulousDebugger.Logger.LogInfo($"Center: {sphere.center}");
            }
            else if (col is CapsuleCollider)
            {
                CapsuleCollider capsule = (CapsuleCollider)col;
                FantabulousDebugger.Logger.LogInfo($"Radius: {capsule.radius}");
                FantabulousDebugger.Logger.LogInfo($"Height: {capsule.height}");
                FantabulousDebugger.Logger.LogInfo($"Direction: {capsule.direction}");
            }
        }
        else if (targetComponent is MeshFilter)
        {
            MeshFilter mf = (MeshFilter)targetComponent;
            FantabulousDebugger.Logger.LogInfo($"Mesh: {(mf.sharedMesh != null ? mf.sharedMesh.name : "None")}");
        }
        else if (targetComponent is MeshRenderer)
        {
            MeshRenderer mr = (MeshRenderer)targetComponent;
            FantabulousDebugger.Logger.LogInfo($"Cast Shadows: {mr.castShadows}");
            FantabulousDebugger.Logger.LogInfo($"Receive Shadows: {mr.receiveShadows}");
            Material[] materials = mr.sharedMaterials;
            string[] materialNames = materials.Select(m => m != null ? m.name : "None").ToArray();
            FantabulousDebugger.Logger.LogInfo($"Materials: {string.Join(", ", materialNames)}");
        }
        else if (targetComponent is Camera)
        {
            Camera cam = (Camera)targetComponent;
            FantabulousDebugger.Logger.LogInfo($"Field of View: {cam.fieldOfView}");
            FantabulousDebugger.Logger.LogInfo($"Near Clip Plane: {cam.nearClipPlane}");
            FantabulousDebugger.Logger.LogInfo($"Far Clip Plane: {cam.farClipPlane}");
            FantabulousDebugger.Logger.LogInfo($"Culling Mask: {cam.cullingMask}");
        }
        else if (targetComponent is Light)
        {
            Light light = (Light)targetComponent;
            FantabulousDebugger.Logger.LogInfo($"Type: {light.type}");
            FantabulousDebugger.Logger.LogInfo($"Color: {light.color}");
            FantabulousDebugger.Logger.LogInfo($"Intensity: {light.intensity}");
            FantabulousDebugger.Logger.LogInfo($"Range: {light.range}");
        }
        else if (targetComponent is AudioSource)
        {
            AudioSource audio = (AudioSource)targetComponent;
            FantabulousDebugger.Logger.LogInfo($"Clip: {(audio.clip != null ? audio.clip.name : "None")}");
            FantabulousDebugger.Logger.LogInfo($"Volume: {audio.volume}");
            FantabulousDebugger.Logger.LogInfo($"Pitch: {audio.pitch}");
            FantabulousDebugger.Logger.LogInfo($"Loop: {audio.loop}");
            FantabulousDebugger.Logger.LogInfo($"Playing: {audio.isPlaying}");
        }
        else
        {
            // Generic component info using reflection for unknown types
            var properties = targetComponent.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            FantabulousDebugger.Logger.LogInfo($"Properties:");
            foreach (var prop in properties.Take(10)) // Limit to first 10 properties
            {
                try
                {
                    var value = prop.GetValue(targetComponent, null);
                    FantabulousDebugger.Logger.LogInfo($"  {prop.Name}: {value}");
                }
                catch
                {
                    FantabulousDebugger.Logger.LogInfo($"  {prop.Name}: [Unable to access]");
                }
            }
        }
        
        FantabulousDebugger.Logger.LogInfo($"=============================");
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
            
            FantabulousDebugger.Logger.LogInfo("Noclip enabled - Use WASD + Space/Ctrl to move");
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

    private static void HandleMaxHealthCommand(string[] commandArgs)
    {
        if (commandArgs.Length == 0)
        {
            FantabulousDebugger.Logger.LogWarning("Usage: maxhealth <health>");
            return;
        }

        if (!int.TryParse(commandArgs[0], out int health))
        {
            FantabulousDebugger.Logger.LogWarning("Invalid health value. Please provide a number.");
            return;
        }

        Stats stats = player.GetComponent<Stats>();
        PlayerPrefs.SetInt("MaxHealth", health);
        stats.health = health;
    }

    private static void HandleWeaponsCommand()
    {
        PlayerPrefs.SetInt("Shot1Unlocked", 1);
        PlayerPrefs.SetInt("Shot2Unlocked", 1);
        PlayerPrefs.SetInt("Shot3Unlocked", 1);

        FantabulousDebugger.Logger.LogInfo("All weapons unlocked");
    }

    private static string HelpForCommand(string command)
    {
        switch (command.ToLower())
        {
            case "help":
                return @"Command: help
Description: Shows available commands or detailed help for specific commands
Usage: 
  help                    - Shows all available commands
  help <command>          - Shows detailed help for specific command

Examples:
  help                    - Lists all commands
  help tp                 - Shows help for teleport command";
                
            case "godmode":
                return @"Command: godmode
Description: Toggles invincibility mode
Usage: godmode

Effects:
  - When enabled: Player health is constantly set to maximum
  - When disabled: Player health returns to normal
  - Useful for: Testing difficult areas, avoiding death
  
Note: Health is automatically restored each frame while enabled";
                
            case "noclip":
                return @"Command: noclip [speed <value>|boost <multiplier>]
Description: Toggles collision-free movement mode or adjusts speed settings
Usage: 
  noclip                    - Toggle noclip mode
  noclip speed <value>      - Set normal movement speed
  noclip boost <multiplier> - Set boost speed as multiplier of normal speed

Effects:
  - When enabled: Player can move through walls and objects
  - When disabled: Normal collision detection restored
  - Movement: WASD + Space (up) + Ctrl (down) + Shift (boost)
  - Default speeds: Normal (50), Boost (100)
  
Speed Settings:
  - speed: Sets the base movement speed (must be > 0)
  - boost: Sets boost speed as multiplier of normal speed (must be > 0)
  - Example: 'noclip speed 75' sets normal speed to 75
  - Example: 'noclip boost 3' sets boost to 3x normal speed

Note: Disables gravity and collision while enabled";
                
            case "tp":
                return @"Command: tp <x> <y> <z> OR tp <object_name>
Description: Teleports player to specified coordinates or to a game object's position
Usage: tp <x> <y> <z> or tp <object_name>

Parameters:
  x, y, z - World coordinates to teleport to
  object_name - Name of game object to teleport to

Examples:
  tp 0 10 5            - Teleport to coordinates (0, 10, 5)
  tp -100.5 2.3 45.2  - Teleport with decimal coordinates
  tp Player             - Teleport to Player object's position
  tp door               - Teleport to door object's position
  tp orangeSphere        - Teleport to orangeSphere object's position

Note: Supports partial name matching for object names";
                
            case "level":
                return @"Command: level <name|index>
Description: Loads specified level
Usage: level <level_name> or level <level_index>

Available Levels:
  0: title
  1: intro
  2: hub
  3: circlefriendfightyplace
  4: ShamrockFakeCastle
  5: signvilleussr
  6: signmanBattle
  7: thefantabula
  8: hubdarkened
  9: prison
  10: Sham5KFight1
  11: ShamrockCastle
  12: ShamrockKingBoss

Examples:
  level hub               - Loads hub level
  level 3                 - Loads circlefriendfightyplace
  level ShamrockCastle     - Loads ShamrockCastle level

Note: Level names are case-insensitive";
                
            case "scan":
                return @"Command: scan [simple|detailed]
Description: Scans and lists all game objects in the scene
Usage: scan or scan simple or scan detailed

Parameters:
  simple    - Shows basic object information (default)
  detailed  - Shows detailed object information

Examples:
  scan                  - Shows basic object list
  scan simple           - Shows basic object list
  scan detailed          - Shows detailed object information

Note: Useful for finding object names for other commands";
                
            case "inspect":
                return @"Command: inspect [object_name] [component_name]
Description: Shows detailed information about game objects or components
Usage: inspect or inspect <object_name> or inspect <object_name> <component>

Parameters:
  object_name   - Name of object to inspect (optional)
  component    - Specific component to inspect (optional)

Examples:
  inspect                    - Inspects object you're looking at
  inspect Player             - Inspects Player object
  inspect Player rigidbody   - Inspects Rigidbody component on Player
  inspect door boxcollider   - Inspects BoxCollider component on door object

Note: Supports partial name matching and component-specific details";
                
            case "fullbright":
                return @"Command: fullbright [on|off]
Description: Toggles enhanced lighting mode and removes fog for maximum visibility
Usage: 
  fullbright           - Toggle fullbright mode on/off
  fullbright on        - Enable fullbright mode
  fullbright off       - Disable fullbright mode

Effects:
  - When enabled: Enhances all scene lighting and removes fog for maximum visibility
  - When disabled: Restores normal lighting and original fog settings
  - Creates directional light if no lights exist
  - Increases intensity of existing lights
  - Completely removes atmospheric fog

Examples:
  fullbright           - Toggle fullbright mode
  fullbright on        - Enable enhanced lighting and remove fog
  fullbright off       - Restore normal lighting and fog

Note: Useful for exploring dark areas, debugging lighting issues, or maximum visibility";
            case "maxhealth":
                return @"Command: maxhealth <health>
Description: Sets the player's maximum health
Usage: maxhealth <health>

Parameters:
  health        - Maximum health value to set

Examples:
  maxhealth 3          - Set maximum health to 3
  maxhealth 10         - Set maximum health to 10

Note: Also sets current health to match the new maximum";
            case "weapons":
                return @"Command: weapons
Description: Unlocks all weapons
Usage: weapons

Note: Unlocks all weapons";

                
            default:
                return $"'{command}' is not a recognized command. Type 'help' to see available commands.";
        }
    }

    private static float noclipSpeed = 30f;
    private static float noclipBoostSpeed = 60f;

    private void OnLevelWasLoaded(int level)
    {
        // Reset debug states during any level transition
        if (noclip)
        {
            ToggleNoclip();
            FantabulousDebugger.Logger.LogInfo("Noclip disabled during level transition");
        }
        
        // Reset fog settings storage since scene will be reset anyway
        fogSettingsStored = false;
        fullbright = false;
    }

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
            
            // Determine current speed (normal or boost)
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? noclipBoostSpeed : noclipSpeed;
            desiredPlayerPosition += moveDirection * currentSpeed * Time.deltaTime;
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
