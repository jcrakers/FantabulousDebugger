using UnityEngine;

namespace FantabulousDebugger;

public class LevelSelectGUI : MonoBehaviour
{
    private bool showLevelSelect = false;
    private Rect windowRect = new Rect(20, 20, 300, 400);
    private string[] levels = { "title", "intro", "hub", "circlefriendfightyplace", "ShamrockFakeCastle", "signvilleussr", "signmanBattle", "thefantabula", "hubdarkened", "prison", "Sham5KFight1", "ShamrockCastle", "ShamrockKingBoss" };
    
    private void Start()
    {
        showLevelSelect = true;
    }
    
    private void Update()
    {

    }
    
    private void OnGUI()
    {
        if (showLevelSelect)
        {
            windowRect = GUI.Window(123456, windowRect, LevelSelectWindow, "Level Select");
        }
    }
    
    private void LevelSelectWindow(int windowID)
    {
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
        
        GUILayout.Label("Select Level:", GUI.skin.box);
        GUILayout.Space(10);
        
        // Create buttons for each level
        for (int i = 0; i < levels.Length; i++)
        {
            if (GUILayout.Button($"{i}: {levels[i]}"))
            {
                LoadLevel(levels[i]);
            }
        }
        
        GUILayout.Space(10);
    }
    
    private void LoadLevel(string levelName)
    {
        try
        {
            FantabulousDebugger.Logger.LogInfo($"Loading level: {levelName}");
            Application.LoadLevel(levelName);
            
            // Unpause and lock cursor
            if (NewDebugConsole.mainGUI != null)
            {
                NewDebugConsole.mainGUI.paused = false;
                Screen.lockCursor = !NewDebugConsole.mainGUI.paused;
            }
            
            showLevelSelect = false;
        }
        catch (System.Exception e)
        {
            FantabulousDebugger.Logger.LogError($"Failed to load level '{levelName}': {e.Message}");
        }
    }
}
