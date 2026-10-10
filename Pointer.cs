using UnityEngine;
using System.Linq;

namespace FantabulousDebugger;

public static class Pointer
{
    private static GameObject pointerObject;
    private static readonly float[][] arrowMeshVertices =
    {
        new float[] {  0.000000f,  1.226251f, -0.582628f },
        new float[] {  0.000000f, -0.001506f,  0.000000f },
        new float[] { -0.276806f,  6.190349f, -0.275926f },
        new float[] {  0.162451f,  1.226251f, -0.162451f },
        new float[] {  0.582628f,  1.226251f, -0.000000f },
        new float[] {  0.162451f,  1.226251f,  0.162451f },
        new float[] {  0.000000f,  1.226251f,  0.582628f },
        new float[] { -0.162451f,  1.226251f,  0.162451f },
        new float[] { -0.582628f,  1.226251f, -0.000000f },
        new float[] {  0.276806f,  6.190349f, -0.275926f },
        new float[] { -0.162451f,  1.226251f, -0.162451f },
        new float[] { -0.276072f,  6.188358f,  0.276072f },
        new float[] {  0.276072f,  6.188359f,  0.276072f },
        new float[] {  0.000000f,  6.191401f,  1.582628f },
        new float[] {  0.000000f,  6.191401f, -1.582628f },
        new float[] { -1.582628f,  6.191401f,  0.000000f },
        new float[] {  1.582628f,  6.191401f,  0.000000f },
        new float[] { -0.000001f,  8.067117f,  0.000001f },
        new float[] {  0.162451f,  6.033812f, -0.162451f },
        new float[] {  0.000000f,  6.040007f,  0.582628f },
        new float[] { -0.582628f,  6.040007f,  0.000000f },
        new float[] {  0.000000f,  6.017790f, -0.582628f },
        new float[] { -0.162451f,  6.033813f, -0.162451f },
        new float[] { -0.162451f,  6.040007f,  0.162451f },
        new float[] {  0.582628f,  6.040007f,  0.000000f },
        new float[] {  0.162451f,  6.040007f,  0.162451f }
    };

    private static readonly int[] arrowMeshTriangles =
    {
        8, 1, 7,
        4, 1, 3,
        7, 1, 6,
        6, 1, 5,
        21, 3, 0,
        4, 25, 5,
        23, 8, 7,
        10, 21, 0,
        18, 4, 3,
        19, 7, 6,
        25, 6, 5,
        8, 22, 10,
        13, 17, 11,
        3, 1, 0,
        5, 1, 4,
        0, 1, 10,
        10, 1, 8,
        9, 17, 16,
        16, 17, 12,
        2, 17, 14,
        15, 17, 2,
        11, 17, 15,
        12, 17, 13,
        14, 17, 9,
        9, 24, 18,
        21, 9, 18,
        19, 11, 23,
        24, 12, 25,
        12, 19, 25,
        11, 20, 23,
        20, 2, 22,
        2, 21, 22,
        21, 18, 3,
        4, 24, 25,
        23, 20, 8,
        10, 22, 21,
        18, 24, 4,
        19, 23, 7,
        25, 19, 6,
        8, 20, 22,
        9, 16, 24,
        21, 14, 9,
        19, 13, 11,
        24, 16, 12,
        12, 13, 19,
        11, 15, 20,
        20, 15, 2,
        2, 14, 21
    };

    public static void HandlePointerCommand(string[] args)
    {
        if (args.Length == 0)
        {
            PrintPointerInfo();
            return;
        }

        string subCommand = args[0].ToLower();

        switch (subCommand)
        {
            case "set":
                HandleSetCommand(args);
                break;
            case "move":
            case "m":
                HandleMoveCommand(args);
                break;
            case "rotate":
            case "r":
                HandleRotateCommand(args);
                break;
            default:
                FantabulousDebugger.Logger.LogWarning($"Unknown pointer subcommand: {subCommand}");
                break;
        }
    }

    private static void PrintPointerInfo()
    {
        if (pointerObject == null)
        {
            FantabulousDebugger.Logger.LogInfo("Pointer does not exist. Use 'pointer set' to create it.");
            return;
        }

        Vector3 pos = pointerObject.transform.position;
        Vector3 rot = pointerObject.transform.rotation.eulerAngles;
        FantabulousDebugger.Logger.LogInfo($"Pointer Position: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
        FantabulousDebugger.Logger.LogInfo($"Pointer Rotation: ({rot.x:F2}, {rot.y:F2}, {rot.z:F2})");
    }

    private static void HandleSetCommand(string[] args)
    {
        Vector3 targetPosition;
        Quaternion targetRotation = Quaternion.identity;

        if (args.Length == 1)
        {
            // No arguments - set at where player is looking
            Quaternion unusedRotation;
            if (!GetPlayerLookPosition(out targetPosition, out unusedRotation))
            {
                return;
            }
            // Don't change rotation for "pointer set"
            if (pointerObject != null)
            {
                targetRotation = pointerObject.transform.rotation;
            }
        }
        else if (args.Length == 4)
        {
            // Three coordinates provided
            if (!float.TryParse(args[1], out float x) ||
                !float.TryParse(args[2], out float y) ||
                !float.TryParse(args[3], out float z))
            {
                FantabulousDebugger.Logger.LogWarning("Invalid coordinates. Usage: pointer set <x> <y> <z>");
                return;
            }
            targetPosition = new Vector3(x, y, z);
            // Don't change rotation for "pointer set <x> <y> <z>"
            if (pointerObject != null)
            {
                targetRotation = pointerObject.transform.rotation;
            }
        }
        else
        {
            // Assume it's a game object name
            string objectName = string.Join(" ", args.Skip(1).ToArray());
            GameObject targetObject = GameObject.Find(objectName);

            if (targetObject == null)
            {
                // Try partial name matching
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

            targetPosition = targetObject.transform.position;
            targetRotation = targetObject.transform.rotation;
        }

        CreateOrUpdatePointer(targetPosition, targetRotation);
    }

    private static void HandleMoveCommand(string[] args)
    {
        if (pointerObject == null)
        {
            FantabulousDebugger.Logger.LogWarning("Pointer does not exist. Use 'pointer set' to create it first.");
            return;
        }

        if (args.Length < 2)
        {
            FantabulousDebugger.Logger.LogWarning("Usage: pointer move <x|y|z> <amount> OR pointer move <object_name> OR pointer move reset");
            return;
        }

        string axis = args[1].ToLower();

        // Check for reset
        if (axis == "reset")
        {
            pointerObject.transform.position = Vector3.zero;
            FantabulousDebugger.Logger.LogInfo("Pointer position reset to (0, 0, 0)");
            return;
        }

        // Check for GameObject name
        if (args.Length == 2)
        {
            string objectName = axis;
            GameObject targetObject = GameObject.Find(objectName);

            if (targetObject == null)
            {
                // Try partial name matching
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

            pointerObject.transform.position = targetObject.transform.position;
            FantabulousDebugger.Logger.LogInfo($"Pointer moved to position of '{targetObject.name}': {targetObject.transform.position}");
            return;
        }

        // Original axis movement logic
        if (args.Length < 3)
        {
            FantabulousDebugger.Logger.LogWarning("Usage: pointer move <x|y|z> <amount>");
            return;
        }

        if (!float.TryParse(args[2], out float amount))
        {
            FantabulousDebugger.Logger.LogWarning("Invalid amount. Usage: pointer move <x|y|z> <amount>");
            return;
        }

        Vector3 movement = Vector3.zero;
        switch (axis)
        {
            case "x":
                movement = new Vector3(amount, 0, 0);
                break;
            case "y":
                movement = new Vector3(0, amount, 0);
                break;
            case "z":
                movement = new Vector3(0, 0, amount);
                break;
            default:
                FantabulousDebugger.Logger.LogWarning($"Invalid axis: {axis}. Use x, y, or z.");
                return;
        }

        pointerObject.transform.position += movement;
        FantabulousDebugger.Logger.LogInfo($"Pointer moved by {axis}: {amount}");
    }

    private static void HandleRotateCommand(string[] args)
    {
        if (pointerObject == null)
        {
            FantabulousDebugger.Logger.LogWarning("Pointer does not exist. Use 'pointer set' to create it first.");
            return;
        }

        if (args.Length < 2)
        {
            FantabulousDebugger.Logger.LogWarning("Usage: pointer rotate <x|y|z> <degrees> OR pointer rotate <object_name> OR pointer rotate reset");
            return;
        }

        string axis = args[1].ToLower();

        // Check for reset
        if (axis == "reset")
        {
            pointerObject.transform.rotation = Quaternion.identity;
            FantabulousDebugger.Logger.LogInfo("Pointer rotation reset to (0, 0, 0)");
            return;
        }

        // Check for GameObject name
        if (args.Length == 2)
        {
            string objectName = axis;
            GameObject targetObject = GameObject.Find(objectName);

            if (targetObject == null)
            {
                // Try partial name matching
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

            pointerObject.transform.rotation = targetObject.transform.rotation;
            Vector3 rot = targetObject.transform.rotation.eulerAngles;
            FantabulousDebugger.Logger.LogInfo($"Pointer rotated to match '{targetObject.name}': ({rot.x:F2}, {rot.y:F2}, {rot.z:F2})");
            return;
        }

        // Original axis rotation logic
        if (args.Length < 3)
        {
            FantabulousDebugger.Logger.LogWarning("Usage: pointer rotate <x|y|z> <degrees>");
            return;
        }

        if (!float.TryParse(args[2], out float degrees))
        {
            FantabulousDebugger.Logger.LogWarning("Invalid degrees. Usage: pointer rotate <x|y|z> <degrees>");
            return;
        }

        Vector3 rotation = Vector3.zero;
        switch (axis)
        {
            case "x":
                rotation = new Vector3(degrees, 0, 0);
                break;
            case "y":
                rotation = new Vector3(0, degrees, 0);
                break;
            case "z":
                rotation = new Vector3(0, 0, degrees);
                break;
            default:
                FantabulousDebugger.Logger.LogWarning($"Invalid axis: {axis}. Use x, y, or z.");
                return;
        }

        pointerObject.transform.Rotate(rotation);
        FantabulousDebugger.Logger.LogInfo($"Pointer rotated by {axis}: {degrees} degrees");
    }

    private static bool GetPlayerLookPosition(out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        Camera playerCamera = Camera.main;
        if (playerCamera == null)
        {
            FantabulousDebugger.Logger.LogWarning("No camera found.");
            return false;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f))
        {
            position = hit.point;
            rotation = Quaternion.LookRotation(hit.normal);
        }
        else
        {
            position = ray.GetPoint(10f);
            rotation = playerCamera.transform.rotation;
        }

        return true;
    }

    private static void CreateOrUpdatePointer(Vector3 position, Quaternion rotation)
    {
        if (pointerObject == null)
        {
            pointerObject = new GameObject("DebugPointer");
            GameObject.DontDestroyOnLoad(pointerObject);

            // Create sphere
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(pointerObject.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 0.75f;

            // Create forward arrow
            CreateArrowMesh(pointerObject.transform, Vector3.zero, Quaternion.Euler(90, 0, 0), Color.yellow);

            // Create up arrow
            CreateArrowMesh(pointerObject.transform, Vector3.zero, Quaternion.identity, Color.cyan);
        }

        pointerObject.transform.position = position;
        pointerObject.transform.rotation = rotation;

        FantabulousDebugger.Logger.LogInfo($"Pointer set to position: {position}");
    }

    private static void CreateArrowMesh(Transform parent, Vector3 localPosition, Quaternion localRotation, Color color)
    {
        GameObject arrowObject = new GameObject("Arrow");
        arrowObject.transform.SetParent(parent);
        arrowObject.transform.localPosition = localPosition;
        arrowObject.transform.localRotation = localRotation;
        arrowObject.transform.localScale = Vector3.one * 0.25f;

        MeshFilter meshFilter = arrowObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = arrowObject.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[arrowMeshVertices.Length];
        for (int i = 0; i < arrowMeshVertices.Length; i++)
        {
            vertices[i] = new Vector3(arrowMeshVertices[i][0], arrowMeshVertices[i][1], arrowMeshVertices[i][2]);
        }

        int[] triangles = arrowMeshTriangles;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        // Create materials for color coding
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = color;
        meshRenderer.material = material;
    }
}
