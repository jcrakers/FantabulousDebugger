using UnityEngine;

namespace FantabulousDebugger;

public static class PlayerDebugMovement
{
    private static bool noclip = false;
    private static Vector3 desiredPlayerPosition;
    private static Collider playerCollider;
    private static Rigidbody playerRigidbody;
    private static GameObject player;
    
    private static float noclipSpeed = 30f;
    private static float noclipBoostSpeed = 60f;
    private static float noclipBoostMultiplier = 2f;

    public static bool Noclip => noclip;
    public static Vector3 DesiredPlayerPosition => desiredPlayerPosition;

    public static void Initialize(GameObject playerRef)
    {
        player = playerRef;
    }

    public static void HandleNoclipCommand(string[] commandArgs)
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
                noclipBoostSpeed = noclipSpeed * noclipBoostMultiplier;
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
                noclipBoostMultiplier = newBoost;
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

    public static void UpdateDesiredPosition(Vector3 position)
    {
        desiredPlayerPosition = position;
    }

    public static void HandleNoclipMovement()
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
                moveDirection += player.transform.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveDirection -= player.transform.forward;
            }
            if (Input.GetKey(KeyCode.A))
            {
                moveDirection -= player.transform.right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveDirection += player.transform.right;
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

    public static void DisableOnLevelLoad()
    {
        if (noclip)
        {
            ToggleNoclip();
            FantabulousDebugger.Logger.LogInfo("Noclip disabled during level transition");
        }
    }
}
