using UnityEngine;
using System.Collections.Generic;

namespace FantabulousDebugger;

public class NewDebugConsole : MonoBehaviour
{
    public static MainGUI mainGUI;

    public static void CreateDeveloperConsole()
    {
        GameObject consoleObject = new GameObject("DeveloperConsole");
        NewDebugConsole console = consoleObject.AddComponent<NewDebugConsole>();
        Commands commands = consoleObject.AddComponent<Commands>();
        GameObject.DontDestroyOnLoad(consoleObject);

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


    static private Vector2 scrollPos;
    private bool consoleVisible = false;
    private bool wasJustOpened = false;
    private Rect windowRect = new Rect(10, 10, Screen.width / 2.5f, Screen.height / 2.5f);
    
    private GUIStyle consoleLabelStyle;
    private GUIStyle consoleTextFieldStyle;

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
    private string currentInput = "";
    private int commandHistoryIndex = 0;
    private List<string> commandHistory = new List<string>();
    
    private void ConsoleWindow(int id)
    {
        // Initialize styles if null
        if (consoleLabelStyle == null)
        {
            consoleLabelStyle = new GUIStyle(GUI.skin.label);
            consoleLabelStyle.fontSize = 12;
            consoleLabelStyle.fontStyle = FontStyle.Normal;
            consoleLabelStyle.margin = new RectOffset(0, 0, 0, 0);
            consoleLabelStyle.padding = new RectOffset(0, 0, 0, 0);
        }
        if (consoleTextFieldStyle == null)
        {
            consoleTextFieldStyle = new GUIStyle(GUI.skin.textField);
            consoleTextFieldStyle.fontSize = 12;
            consoleTextFieldStyle.fontStyle = FontStyle.Normal;
        }

        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                if (!string.IsNullOrEmpty(currentInput))
                {
                    // Process the command
                    Commands.ExecuteCommand(currentInput.ToLower());
                    commandHistory.Add(currentInput);
                    commandHistoryIndex = commandHistory.Count;
                    currentInput = "";

                    // Keep the history at a reasonable size
                    if (commandHistory.Count > 500)
                    {
                        commandHistory.RemoveAt(0);
                    }
                    
                    // Optional: Keeps focus in the box after hitting enter
                    GUI.FocusControl("consoleInput"); 
                }
                // Consume the event so it doesn't trigger other logic
                e.Use();
            }
            if (e.keyCode == KeyCode.UpArrow)
            {
                if (commandHistory.Count > 0 && commandHistoryIndex > 0)
                    {
                        commandHistoryIndex--;
                        currentInput = commandHistory[commandHistoryIndex];
                        // Move cursor to end of line
                        TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                        if (te != null)
                        {
                            te.pos = currentInput.Length;
                            te.selectPos = currentInput.Length;
                        }
                    }
                e.Use();
            }
            if (e.keyCode == KeyCode.DownArrow)
            {
                if (commandHistoryIndex < commandHistory.Count - 1)
                {
                    commandHistoryIndex++;
                    currentInput = commandHistory[commandHistoryIndex];
                    // Move cursor to end of line
                    TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (te != null)
                    {
                        te.pos = currentInput.Length;
                        te.selectPos = currentInput.Length;
                    }
                }
                e.Use();
            }
            if (e.keyCode == KeyCode.Escape)
            {
                consoleVisible = false;
                GUI.FocusControl(null);
                mainGUI.paused = false;
                Screen.lockCursor = !mainGUI.paused;
                e.Use();
            }
        }
    
        GUILayout.BeginVertical();
        
        float padding = 10f;
        float textFieldHeight = 25f;
        float titleHeight = 20f;
    
        float scrollWidth = windowRect.width - (padding * 2);
        float scrollHeight = windowRect.height - titleHeight - textFieldHeight - (padding * 1.2f);

        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(scrollWidth), GUILayout.Height(scrollHeight));
        foreach (string log in FantabulousDebugger.logHistory) {
            if (log.StartsWith("[Error") || log.StartsWith("[Exception")) {
                GUI.color = Color.red;
            } else if (log.StartsWith("[Warning")) {
                GUI.color = Color.yellow;
            } else {
                GUI.color = Color.white;
            }
            GUILayout.Label(log, consoleLabelStyle);
        }

        if (FantabulousDebugger.logHistory.Count != LogCount)
        {
            LogCount = FantabulousDebugger.logHistory.Count;
            scrollPos.y = float.MaxValue;
        }

        GUI.color = Color.white;
        GUILayout.EndScrollView();

        GUI.SetNextControlName("consoleInput");

        currentInput = GUILayout.TextField(currentInput, GUILayout.Height(textFieldHeight));

        GUILayout.EndVertical();

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
        }
    }
}
