using UnityEngine;

namespace FantabulousDebugger;

public static class TeleportSystem
{
    private static GameObject player;

    public static void Initialize(GameObject playerRef)
    {
        player = playerRef;
    }

    public static void HandleTeleportCommand(string[] commandArgs)
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
    
    public static void PerformTeleport(Vector3 targetPosition)
    {
        // Update player position
        player.transform.position = targetPosition;
        
        // Reset velocity to prevent physics issues
        if (player.GetComponent<Rigidbody>() != null)
        {
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        
        // Update noclip desired position if noclip is enabled
        PlayerDebugMovement.UpdateDesiredPosition(targetPosition);
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
}
