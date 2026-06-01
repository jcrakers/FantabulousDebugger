using UnityEngine;
using System.Linq;

namespace FantabulousDebugger;

public static class VisualEffects
{
    private static bool fullbright = false;
    private static bool wireframeMode = false;
    private static bool drawColliders = false;
    
    // Fog settings storage
    private static bool originalFogState;
    private static Color originalFogColor;
    private static float originalFogDensity;
    private static float originalFogStartDistance;
    private static float originalFogEndDistance;
    private static bool fogSettingsStored = false;

    public static bool Fullbright => fullbright;
    public static bool WireframeMode => wireframeMode;
    public static bool DrawColliders => drawColliders;

    public static void Reset()
    {
        fullbright = false;
        wireframeMode = false;
        drawColliders = false;
        fogSettingsStored = false;
    }

    public static void HandleFullbrightCommand(string[] commandArgs)
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
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        
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
                    GameObject.DestroyImmediate(light.gameObject);
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

    public static void HandleWireframeCommand()
    {
        // Toggle wireframe mode
        wireframeMode = !wireframeMode;
        FantabulousDebugger.Logger.LogInfo($"Wireframe mode: {(wireframeMode ? "ON" : "OFF")}");
        
        if (Camera.main != null)
        {
            if (Camera.main.gameObject.GetComponent<Wireframe>() == null && wireframeMode)
            {
                Camera.main.gameObject.AddComponent<Wireframe>();
            }
            else if (Camera.main.gameObject.GetComponent<Wireframe>() != null && !wireframeMode)
            {
                GameObject.Destroy(Camera.main.gameObject.GetComponent<Wireframe>());
            }
        }
    }
    
    public static void HandleCollidersCommand()
    {
        // Toggle colliders drawing
        drawColliders = !drawColliders;
        FantabulousDebugger.Logger.LogInfo($"Colliders drawing: {(drawColliders ? "ON" : "OFF")}");
    }
}

public class Wireframe : MonoBehaviour
{
    void OnPreRender()
    {
        if (VisualEffects.WireframeMode)
        {
            GL.wireframe = true;
        }
        else
        {
            GL.wireframe = false;
        }
    }
    void OnPostRender()
    {
        GL.wireframe = false;
    }
}
