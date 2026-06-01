using UnityEngine;
using System.Linq;

namespace FantabulousDebugger;

public static class ObjectInspector
{
    private static GameObject player;

    public static void Initialize(GameObject playerRef)
    {
        player = playerRef;
    }

    public static void HandleInspectCommand(string[] commandArgs)
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
}
