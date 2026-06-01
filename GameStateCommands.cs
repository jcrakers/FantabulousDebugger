using UnityEngine;
using System.Linq;

namespace FantabulousDebugger;

public static class GameStateCommands
{
    private static bool godmode = false;
    private static bool machineGun = false;
    private static float fireRate = 0.05f;
    private static GameObject player;
    private static GameObject sphereThrower;
    private static ThrowBall throwBall;
    private static float internalCooldown = 0f;

    public static bool Godmode => godmode;
    public static bool MachineGun => machineGun;

    public static void Initialize(GameObject playerRef)
    {
        player = playerRef;
    }

    public static void HandleGodmodeCommand()
    {
        godmode = !godmode;
        FantabulousDebugger.Logger.LogInfo($"Godmode: {godmode}");
    }

    public static void HandleMaxHealthCommand(string[] commandArgs)
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

    public static void HandleWeaponsCommand()
    {
        PlayerPrefs.SetInt("Shot1Unlocked", 1);
        PlayerPrefs.SetInt("Shot2Unlocked", 1);
        PlayerPrefs.SetInt("Shot3Unlocked", 1);

        FantabulousDebugger.Logger.LogInfo("All weapons unlocked");
    }

    public static void HandleAddSausageCommand(string[] commandArgs)
    {
        int addsausage = 1;
        if (commandArgs.Length > 0 && !int.TryParse(commandArgs[0], out addsausage))
        {
            FantabulousDebugger.Logger.LogWarning("Invalid sausage value. Please provide a number.");
            return;
        }
        if (addsausage < 1)
        {
            FantabulousDebugger.Logger.LogWarning("Sausage value must be greater than 0.");
            return;
        }

        Stats stats = player.GetComponent<Stats>();

        stats.sausageCount += addsausage;
        stats.totalSausages += addsausage;
        int sausages = PlayerPrefs.GetInt("Sausages");
        PlayerPrefs.SetInt("Sausages", sausages + addsausage);

        FantabulousDebugger.Logger.LogInfo($"Added {addsausage} sausage(s). Total sausages: {sausages + addsausage}");
    }

    public static void HandleDestroyCommand(string[] commandArgs)
    {
        GameObject targetObject = null;
        if (commandArgs.Length == 1)
        {
            string objectName = commandArgs[0];
            
            // Try to find the object by name
            targetObject = GameObject.Find(objectName);
            
            if (targetObject == null)
            {
                // Try to find in all objects
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
        
        // Destroy the object
        GameObject.Destroy(targetObject);
        FantabulousDebugger.Logger.LogInfo($"Destroyed object: {targetObject.name}");
    }

    public static void HandleMachineGunCommand(string[] commandArgs)
    {
        float desiredFireRate = 0f;
        if (commandArgs.Length > 0)
        {
            if (float.TryParse(commandArgs[0], out float rate))
            {
                desiredFireRate = 1 / rate;
            }
        }
        
        if (desiredFireRate != 0)
        {
            fireRate = desiredFireRate;
            FantabulousDebugger.Logger.LogInfo($"Fire rate set to: {fireRate}");
            machineGun = true;
        }
        else
        {
            machineGun = !machineGun;
        }
        
        FantabulousDebugger.Logger.LogInfo($"Machine gun: {machineGun}");
    }

    public static void Update()
    {
        if (sphereThrower == null)
        {
            sphereThrower = GameObject.Find("SphereThrower");
        }
        else
        {
            if (throwBall == null)
            {
                throwBall = sphereThrower.GetComponent<ThrowBall>();
            }
            else
            {
                if (machineGun && Input.GetButton("Fire1"))
                {
                    HandleMachineGun();
                }
            }
        }
    }

    private static void HandleMachineGun()
    {
        if (internalCooldown >= fireRate)
        {
            GameObject gameObject = (GameObject)GameObject.Instantiate(throwBall.projectile, Camera.main.transform.position, Camera.main.transform.rotation);
            gameObject.rigidbody.velocity = throwBall.transform.forward * 150f;
            internalCooldown = 0f;
        }
        else
        {
            internalCooldown += Time.deltaTime;
        }
    }

    public static void ApplyGodmode()
    {
        if (godmode)
        {
            Stats stats = player.GetComponent<Stats>();
            stats.health = stats.maxHealth;
        }
    }
}
