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
        
        if (Camera.main != null)
        {
            if (Camera.main.gameObject.GetComponent<CollidersDrawer>() == null && drawColliders)
            {
                Camera.main.gameObject.AddComponent<CollidersDrawer>();
            }
            else if (Camera.main.gameObject.GetComponent<CollidersDrawer>() != null && !drawColliders)
            {
                GameObject.Destroy(Camera.main.gameObject.GetComponent<CollidersDrawer>());
            }
        }
    }
}

public class CollidersDrawer : MonoBehaviour
{
    private readonly Color boxColor = Color.red;
    private readonly Color circleColor = Color.blue;
    private readonly Color capsuleColor = Color.green;
    private readonly Color meshColor = Color.magenta;
    private readonly Color triggerColor = new Color(1f, 0.5f, 0f); // Orange

    private float lastUpdateTime = 0f;
    private readonly float updateInterval = 0.25f;

    Collider[] colliders;

    void Start()
    {
        colliders = Object.FindObjectsOfType<Collider>();
        CreateDebugMaterial();
    }

    private static Material debugLineMaterial;
    public static void CreateDebugMaterial()
    {
        if (debugLineMaterial != null) return;

        // A low-level shader string that disables lighting, culling, and depth testing
        /*string shaderSource = @"
        Shader ""Hidden/ConsoleDebugLines"" {
            SubShader {
                Tags { ""RenderType""=""Transparent"" ""Queue""=""Overlay+100"" }
                Pass {
                    Blend SrcAlpha OneMinusSrcAlpha
                    ZWrite Off
                    ZTest Always
                    Cull Off
                    Lighting Off
                    BindChannels {
                        Bind ""Color"", color
                        Bind ""Vertex"", vertex
                    }
                }
            }
        }";*/

        // Instantiate using the legacy shader compilation constructor
        Shader compiledShader = Shader.Find("Sprites/Default");
        
        // If the engine hasn't registered it yet, compile it from the string logic
        if (compiledShader == null)
        {
            // For older Unity, constructing a material directly with a shader string
            // or using custom material parsing is required if Shader.Find fails.
            // Fallback approach if custom string compilation is restricted by engine strip:
            compiledShader = Shader.Find("Lines/Colored");
        }

        if (compiledShader != null)
        {
            debugLineMaterial = new Material(compiledShader);
            debugLineMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        else
        {
            // Last-resort fallback for older mobile pipelines
            debugLineMaterial = new Material(Shader.Find("Mobile/Particles/Additive"));
        }
    }

    void Update()
    {
        if (VisualEffects.DrawColliders)
        {
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                colliders = Object.FindObjectsOfType<Collider>();
                lastUpdateTime = Time.time;
            }
        }
    }

    // Permanent reusable arrays to prevent allocation spikes
    private readonly Vector3[] localBoxCorners = new Vector3[8];
    private Camera cachedCamera;

    void OnRenderObject()
    {
        if (!VisualEffects.DrawColliders || colliders == null) return;

        CreateDebugMaterial();
        if (debugLineMaterial == null) return;

        // Cache the camera once for this entire render loop
        if (cachedCamera == null) cachedCamera = Camera.main;
        if (cachedCamera == null) return;

        // Isolate rendering states and set up the material step ONCE
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);
        debugLineMaterial.SetPass(0);

        // Open ONE global pipeline stream for the entire frame pass
        GL.Begin(GL.LINES);

        foreach (Collider collider in colliders)
        {
            if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy) continue;

            // Determine color
            Color drawColor = collider.isTrigger ? triggerColor : collider switch
            {
                BoxCollider => boxColor,
                SphereCollider => circleColor,
                CapsuleCollider => capsuleColor,
                MeshCollider => meshColor,
                _ => boxColor
            };

            // Set active line color in the open pipeline stream
            GL.Color(drawColor);

            // Emit vertices directly into the stream without reopening GL.Begin
            if (collider is BoxCollider box) DrawBoxCollider(box);
            else if (collider is SphereCollider sphere) DrawSphereCollider(sphere);
            else if (collider is CapsuleCollider capsule) DrawCapsuleCollider(capsule);
            else if (collider is MeshCollider mesh) DrawMeshCollider(mesh);
        }

        // Close the pipeline stream exactly once
        GL.End();
        GL.PopMatrix();
    }
    
    private void DrawBoxCollider(BoxCollider box)
    {
        Vector3 center = box.center;
        Vector3 size = box.size * 0.5f; // Half-extents

        Vector3[] corners = new Vector3[]
        {
            center + new Vector3(-size.x, -size.y, -size.z),
            center + new Vector3(size.x, -size.y, -size.z),
            center + new Vector3(size.x, size.y, -size.z),
            center + new Vector3(-size.x, size.y, -size.z),
            center + new Vector3(-size.x, -size.y, size.z),
            center + new Vector3(size.x, -size.y, size.z),
            center + new Vector3(size.x, size.y, size.z),
            center + new Vector3(-size.x, size.y, size.z)
        };

        // Transform local corners to World Space
        Vector3[] worldCorners = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            worldCorners[i] = box.transform.TransformPoint(corners[i]);
        }
        
        // Draw edges of the box
        // Bottom face
        DrawLine(worldCorners[0], worldCorners[1]);
        DrawLine(worldCorners[1], worldCorners[2]);
        DrawLine(worldCorners[2], worldCorners[3]);
        DrawLine(worldCorners[3], worldCorners[0]);
        
        // Top face
        DrawLine(worldCorners[4], worldCorners[5]);
        DrawLine(worldCorners[5], worldCorners[6]);
        DrawLine(worldCorners[6], worldCorners[7]);
        DrawLine(worldCorners[7], worldCorners[4]);
        
        // Vertical edges
        DrawLine(worldCorners[0], worldCorners[4]);
        DrawLine(worldCorners[1], worldCorners[5]);
        DrawLine(worldCorners[2], worldCorners[6]);
        DrawLine(worldCorners[3], worldCorners[7]);
    }
    
    private void DrawSphereCollider(SphereCollider sphere)
    {
        Vector3 worldCenter = sphere.transform.TransformPoint(sphere.center);

        float lossyScale = Mathf.Max(Mathf.Abs(sphere.transform.lossyScale.x), Mathf.Max(Mathf.Abs(sphere.transform.lossyScale.y), Mathf.Abs(sphere.transform.lossyScale.z)));
        float radius = sphere.radius * lossyScale;

        int segments = 20;
        float angleStep = (2f * Mathf.PI) / segments;
        
        // Draw 3 orthogonal circles
        for (int i = 0; i < segments; i++)
        {
            float a1 = i * angleStep;
            float a2 = (i + 1) * angleStep;

            // XY Plane Circle
            DrawLine(worldCenter + new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0),
                    worldCenter + new Vector3(Mathf.Cos(a2) * radius, Mathf.Sin(a2) * radius, 0));

            // XZ Plane Circle
            DrawLine(worldCenter + new Vector3(Mathf.Cos(a1) * radius, 0, Mathf.Sin(a1) * radius),
                    worldCenter + new Vector3(Mathf.Cos(a2) * radius, 0, Mathf.Sin(a2) * radius));

            // YZ Plane Circle
            DrawLine(worldCenter + new Vector3(0, Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius),
                    worldCenter + new Vector3(0, Mathf.Cos(a2) * radius, Mathf.Sin(a2) * radius));
        }
    }
    
    private void DrawCapsuleCollider(CapsuleCollider capsule)
    {
        Transform t = capsule.transform;
        Vector3 center = capsule.center;
        
        // Account for lossy scale based on the orientation axis
        Vector3 globalScale = t.lossyScale;
        float maxRadiusScale = Mathf.Max(Mathf.Abs(globalScale.x), Mathf.Abs(globalScale.z)); // Default for Y-axis
        if (capsule.direction == 0) maxRadiusScale = Mathf.Max(Mathf.Abs(globalScale.y), Mathf.Abs(globalScale.z));
        if (capsule.direction == 2) maxRadiusScale = Mathf.Max(Mathf.Abs(globalScale.x), Mathf.Abs(globalScale.y));

        float radius = capsule.radius * maxRadiusScale;
        float height = capsule.height * Mathf.Abs(globalScale[capsule.direction]);

        // Calculate the straight cylinder height
        float halfCylinderHeight = Mathf.Max(0f, (height * 0.5f) - radius);

        // Determine the local direction vector based on Unity's axis index
        Vector3 localDir = Vector3.up; // Default: 1 (Y-Axis)
        if (capsule.direction == 0) localDir = Vector3.right;
        else if (capsule.direction == 2) localDir = Vector3.forward;

        // Locate the two cap center points in world space
        Vector3 localCap1 = center + (localDir * halfCylinderHeight);
        Vector3 localCap2 = center - (localDir * halfCylinderHeight);

        Vector3 worldCap1 = t.TransformPoint(localCap1);
        Vector3 worldCap2 = t.TransformPoint(localCap2);

        // Establish dynamic orthogonal axes for drawing the wireframe rings
        Vector3 crossAxis1 = Vector3.forward;
        Vector3 crossAxis2 = Vector3.right;

        if (capsule.direction == 0) // X-Axis capsule
        {
            crossAxis1 = Vector3.up;
            crossAxis2 = Vector3.forward;
        }
        else if (capsule.direction == 2) // Z-Axis capsule
        {
            crossAxis1 = Vector3.up;
            crossAxis2 = Vector3.right;
        }

        DrawCapsuleWireframe(worldCap1, worldCap2, crossAxis1, crossAxis2, radius);
    }
    
    private void DrawMeshCollider(MeshCollider meshCol)
    {
        Mesh mesh = meshCol.sharedMesh;
        if (mesh == null) return;

        Transform t = meshCol.transform;
        Vector3[] vertices = mesh.vertices;

        // A mesh can contain multiple submeshes (materials slots)
        for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
        {
            int[] triangles = mesh.GetTriangles(submesh);

            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Extract the 3 world-space vertices for the current triangle face
                Vector3 vA = t.TransformPoint(vertices[triangles[i]]);
                Vector3 vB = t.TransformPoint(vertices[triangles[i + 1]]);
                Vector3 vC = t.TransformPoint(vertices[triangles[i + 2]]);

                // Draw the 3 edges of the triangle face
                GL.Vertex(vA); GL.Vertex(vB);
                GL.Vertex(vB); GL.Vertex(vC);
                GL.Vertex(vC); GL.Vertex(vA);
            }
        }
    }

    private void DrawLine(Vector3 start, Vector3 end)
    {
        GL.Vertex(start);
        GL.Vertex(end);
    }

    private void DrawCapsuleWireframe(Vector3 cap1, Vector3 cap2, Vector3 axis1, Vector3 axis2, float r)
    {
        int segments = 20;
        float angleStep = (2f * Mathf.PI) / segments;

        // 1. Draw the two full circular bases where the cylinder meets the caps
        for (int i = 0; i < segments; i++)
        {
            float a1 = i * angleStep;
            float a2 = (i + 1) * angleStep;

            Vector3 p1_offset = (axis1 * Mathf.Cos(a1) + axis2 * Mathf.Sin(a1)) * r;
            Vector3 p2_offset = (axis1 * Mathf.Cos(a2) + axis2 * Mathf.Sin(a2)) * r;

            // Circle on Cap 1
            DrawLine(cap1 + p1_offset, cap1 + p2_offset);
            // Circle on Cap 2
            DrawLine(cap2 + p1_offset, cap2 + p2_offset);
        }

        // 2. Draw the 4 side-wall straight lines of the cylinder length
        DrawLine(cap1 + (axis1 * r), cap2 + (axis1 * r));
        DrawLine(cap1 - (axis1 * r), cap2 - (axis1 * r));
        DrawLine(cap1 + (axis2 * r), cap2 + (axis2 * r));
        DrawLine(cap1 - (axis2 * r), cap2 - (axis2 * r));

        // 3. Draw the dome cap arcs (180-degree half-circles)
        float halfAngleStep = Mathf.PI / (segments / 2);
        for (int i = 0; i < segments / 2; i++)
        {
            float a1 = i * halfAngleStep;
            float a2 = (i + 1) * halfAngleStep;

            // Get direction vectors from cap center pointing outwards towards the dome tip
            Vector3 domeDir = Vector3.Normalize(cap1 - cap2); 
            if (domeDir == Vector3.zero) domeDir = Vector3.up; // Fallback if height is 0

            // Cap 1 Outer Arcs
            Vector3 arc1_p1 = (axis1 * Mathf.Cos(a1) + domeDir * Mathf.Sin(a1)) * r;
            Vector3 arc1_p2 = (axis1 * Mathf.Cos(a2) + domeDir * Mathf.Sin(a2)) * r;
            DrawLine(cap1 + arc1_p1, cap1 + arc1_p2);

            Vector3 arc2_p1 = (axis2 * Mathf.Cos(a1) + domeDir * Mathf.Sin(a1)) * r;
            Vector3 arc2_p2 = (axis2 * Mathf.Cos(a2) + domeDir * Mathf.Sin(a2)) * r;
            DrawLine(cap1 + arc2_p1, cap1 + arc2_p2);

            // Cap 2 Outer Arcs (Inverted dome direction)
            Vector3 arc3_p1 = (axis1 * Mathf.Cos(a1) - domeDir * Mathf.Sin(a1)) * r;
            Vector3 arc3_p2 = (axis1 * Mathf.Cos(a2) - domeDir * Mathf.Sin(a2)) * r;
            DrawLine(cap2 + arc3_p1, cap2 + arc3_p2);

            Vector3 arc4_p1 = (axis2 * Mathf.Cos(a1) - domeDir * Mathf.Sin(a1)) * r;
            Vector3 arc4_p2 = (axis2 * Mathf.Cos(a2) - domeDir * Mathf.Sin(a2)) * r;
            DrawLine(cap2 + arc4_p1, cap2 + arc4_p2);
        }
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
