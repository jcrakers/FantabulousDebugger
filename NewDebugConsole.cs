using UnityEngine;

namespace FantabulousDebugger;

public class NewDebugConsole : MonoBehaviour
{
    public static GameObject player;
    public static MainGUI mainGUI;

    public static void CreateDeveloperConsole()
    {
        GameObject consoleObject = new GameObject("DeveloperConsole");
        NewDebugConsole console = consoleObject.AddComponent<NewDebugConsole>();
        GameObject.DontDestroyOnLoad(consoleObject);

        player = GameObject.Find("Player");
        mainGUI = Object.FindObjectOfType<MainGUI>();

        FantabulousDebugger.Logger.LogInfo("Developer console created");
    }

    private void Update()
    {
        // Toggle the console
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (!mainGUI.paused){
                mainGUI.paused = !mainGUI.paused;
                Screen.lockCursor = !mainGUI.paused;
            }
            currentInput = "";
            consoleVisible = !consoleVisible;
            wasJustOpened = true;
        }
        if (!mainGUI.paused)
        {
            consoleVisible = false;
        }
    }

    private string currentInput = "";
    static private Vector2 scrollPos;
    private bool consoleVisible = false;
    private bool wasJustOpened = false;
    private Rect windowRect = new Rect(10, 10, Screen.width / 2.5f, Screen.height / 2.5f);

    private void OnGUI()
    {
        if (consoleVisible)
        {
            GUI.depth = -100;
            
            windowRect = GUI.Window(0, windowRect, ConsoleWindow, "Developer Console");
        }
    }

    private int LogCount = 0;
    private bool isResizing = false;
    
    private void ConsoleWindow(int id)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentInput != "")
            {
                /*string returnValue = ExecuteCommand(currentInput);
                FantabulousDebugger.Logger.LogInfo(returnValue);*/
                currentInput = "";
            }
        }
    
        float padding = 10f;
        float textFieldHeight = 25f;
        float titleHeight = 20f;
    
        float scrollWidth = windowRect.width - (padding * 2);
        float scrollHeight = windowRect.height - titleHeight - textFieldHeight - (padding * 1.2f);

        scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Width(scrollWidth), GUILayout.Height(scrollHeight));
        foreach (string log in FantabulousDebugger.logHistory) {
            if (log.StartsWith("[Error") || log.StartsWith("[Exception")) {
                GUI.color = Color.red;
            } else if (log.StartsWith("[Warning")) {
                GUI.color = Color.yellow;
            } else {
                GUI.color = Color.white;
            }
            GUILayout.Label(log);
        }

        if (FantabulousDebugger.logHistory.Count != LogCount)
        {
            LogCount = FantabulousDebugger.logHistory.Count;
            scrollPos.y = float.MaxValue;
        }

        GUI.color = Color.white;
        GUILayout.EndScrollView();

        GUI.SetNextControlName("consoleInput");
        currentInput = GUI.TextField(new Rect(padding, windowRect.height - textFieldHeight - padding, windowRect.width - (padding * 2), textFieldHeight), currentInput, GUI.skin.textField);

        Rect resizeHandle = new Rect(windowRect.width - 20, windowRect.height - 20, 20, 20);
        GUI.color = Color.blue;
        GUI.Box(new Rect(windowRect.width - 15, windowRect.height - 15, 15, 15), "");
        GUI.color = Color.white;

        if (Event.current.type == EventType.MouseDown && resizeHandle.Contains(Event.current.mousePosition))
        {
            isResizing = true;
            Event.current.Use();
        }

        if (isResizing)
        {
            windowRect.width = Event.current.mousePosition.x + 10;
            windowRect.height = Event.current.mousePosition.y + 10;

            if (windowRect.width < 200) windowRect.width = 200;
            if (windowRect.height < 150) windowRect.height = 150;

            if (Event.current.type == EventType.MouseUp)
            {
                isResizing = false;
            }
        }

        GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));

        if (windowRect.x < 0 - (windowRect.width * 0.5f))
            windowRect.x = 0 - (windowRect.width * 0.5f);

        if (windowRect.x > Screen.width - (windowRect.width * 0.5f))
            windowRect.x = Screen.width - (windowRect.width * 0.5f);
        
        if (windowRect.y < 0 - (windowRect.height * 0.5f))
            windowRect.y = 0 - (windowRect.height * 0.5f);
        
        
        if (windowRect.y > Screen.height - (windowRect.height * 0.5f))
            windowRect.y = Screen.height - (windowRect.height * 0.5f);

        if (wasJustOpened)
        {
            GUI.FocusControl("consoleInput");
            wasJustOpened = false;

            windowRect = new Rect(10, 10, Screen.width / 2.5f, Screen.height / 2.5f);
        }
    }
}
