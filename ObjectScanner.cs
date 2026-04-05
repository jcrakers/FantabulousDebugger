using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FantabulousDebugger;

public static class ObjectScanner
{
    public static void PrintGameObjects(bool activeOnly)
    {
        List<string> gameObjectsList = [];

        // Find all game objects
        if (activeOnly)
        {
            GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
            FantabulousDebugger.Logger.LogInfo($"Found {gameObjects.Length} active game objects");
            foreach (var go in gameObjects)
            {
                gameObjectsList.Add(go.name);
            }
        }
        else
        {
            GameObject[] gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
            FantabulousDebugger.Logger.LogInfo($"Found {gameObjects.Length} total game objects (including inactive)");
            foreach (var go in gameObjects)
            {
                gameObjectsList.Add(go.name);
            }
        }

        // Count occurrences and create sorted list with counts
        Dictionary<string, int> gameObjectCounts = [];
        foreach (var go in gameObjectsList)
        {
            if (gameObjectCounts.ContainsKey(go))
            {
                gameObjectCounts[go]++;
            }
            else
            {
                gameObjectCounts[go] = 1;
            }
        }
        
        // Sort by name and create list with counts
        var sortedNames = gameObjectCounts.Keys.ToList();
        sortedNames.Sort();
        
        // Print the list with counts
        foreach (var name in sortedNames)
        {
            int count = gameObjectCounts[name];
            string displayText = count > 1 ? $"{name} ({count})" : name;
            FantabulousDebugger.Logger.LogInfo(displayText);
        }
    }

    public static Component[] FindObjectComponents(string objectName)
    {
        GameObject[] gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
        foreach (var go in gameObjects)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (var component in components)
            {
                if (go.name == objectName)
                {
                    return component.GetComponents<Component>();
                }
            }
        }
        return null;
    }
}