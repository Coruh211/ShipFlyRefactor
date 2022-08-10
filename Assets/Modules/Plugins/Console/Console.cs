using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
    [SerializeField] private Button _uiButton;

    public bool openOnStart = false;
    public bool shakeToOpen = true;
    private bool onlyLastMessage = false;
    public bool shakeRequiresTouch = false;
    public bool restrictLogCount = false;

    public float scaleFactor = 1f;
    public float shakeAcceleration = 3f;
    public float toggleThresholdSeconds = .5f;
    float lastToggleTime;

    public int maxLogCount = 1000;
    public int logFontSize = 12;

    static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
    static readonly GUIContent onlyOne = new GUIContent("Show Only Last Message", "Show only last messages.");
    static readonly GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");
    const int margin = 20;
    const string windowTitle = "Console";

    private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
        {
            { LogType.Assert, Color.white },
            { LogType.Error, Color.red },
            { LogType.Exception, Color.red },
            { LogType.Log, Color.white },
            { LogType.Warning, Color.yellow },
        };

    bool isCollapsed;
    bool isVisible;
    readonly List<Log> logs = new List<Log>();
    readonly ConcurrentQueue<Log> queuedLogs = new ConcurrentQueue<Log>();

    Vector2 scrollPosition;
    readonly Rect titleBarRect = new Rect(0, 0, 10000, 20);
    float windowX = margin;
    float windowY = margin;

    private readonly Dictionary<LogType, bool> logTypeFilters = new Dictionary<LogType, bool>
    {
            { LogType.Assert, true },
            { LogType.Error, true },
            { LogType.Exception, true },
            { LogType.Log, true },
            { LogType.Warning, true },
    };

    private void OnGUI()
    {
        if (!isVisible)
            return;

        GUI.matrix = Matrix4x4.Scale(Vector3.one * scaleFactor);

        float width = (Screen.width / scaleFactor) - (margin * 2);
        float height = (Screen.height / scaleFactor) - (margin * 2);
        Rect windowRect = new Rect(windowX, windowY, width, height);

        Rect newWindowRect = GUILayout.Window(123456, windowRect, DrawWindow, windowTitle);
        windowX = newWindowRect.x;
        windowY = newWindowRect.y;
    }

    private void Start()
    {
        if (openOnStart)
            isVisible = true;

        if (_uiButton)
            _uiButton.onClick.AddListener(OpenConsoleOnButtonUI);
    }

    private void Update()
    {
        UpdateQueuedLogs();

        float curTime = Time.realtimeSinceStartup;

        if (Input.GetKeyDown(toggleKey))
            isVisible = !isVisible;

        if (shakeToOpen &&
            Input.acceleration.sqrMagnitude > shakeAcceleration &&
            curTime - lastToggleTime >= toggleThresholdSeconds &&
            (!shakeRequiresTouch || Input.touchCount > 2))
        {
            isVisible = !isVisible;
            lastToggleTime = curTime;
        }
    }

    private void DrawLog(Log log, GUIStyle logStyle, GUIStyle badgeStyle)
    {
        GUI.contentColor = logTypeColors[log.type];

        if (isCollapsed)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(log.GetTruncatedMessage(), logStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(log.count.ToString(), GUI.skin.box);
            GUILayout.EndHorizontal();
        }
        else
            for (var i = 0; i < log.count; i += 1)
                GUILayout.Label(log.GetTruncatedMessage(), logStyle);

        GUI.contentColor = Color.white;
    }

    private void DrawLogList()
    {
        GUIStyle badgeStyle = GUI.skin.box;
        badgeStyle.fontSize = logFontSize;

        GUIStyle logStyle = GUI.skin.label;
        logStyle.fontSize = logFontSize;

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.BeginVertical();

        var visibleLogs = logs.Where(IsLogVisible);
        if (onlyLastMessage)
            visibleLogs = new List<Log> { visibleLogs.Last() };

        foreach (Log log in visibleLogs)
            DrawLog(log, logStyle, badgeStyle);

        GUILayout.EndVertical();
        var innerScrollRect = GUILayoutUtility.GetLastRect();
        GUILayout.EndScrollView();
        var outerScrollRect = GUILayoutUtility.GetLastRect();

        if (Event.current.type == EventType.Repaint && IsScrolledToBottom(innerScrollRect, outerScrollRect))
            ScrollToBottom();
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(clearLabel))
            logs.Clear();

        foreach (LogType logType in Enum.GetValues(typeof(LogType)))
        {
            var currentState = logTypeFilters[logType];
            var label = logType.ToString();
            logTypeFilters[logType] = GUILayout.Toggle(currentState, label, GUILayout.ExpandWidth(false));
            GUILayout.Space(20);
        }

        isCollapsed = GUILayout.Toggle(isCollapsed, collapseLabel, GUILayout.ExpandWidth(false));
        onlyLastMessage = GUILayout.Toggle(onlyLastMessage, onlyOne, GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();
    }

    private void DrawWindow(int windowID)
    {
        DrawLogList();
        DrawToolbar();
        GUI.DragWindow(titleBarRect);
    }

    private Log? GetLastLog()
    {
        if (logs.Count == 0)
            return null;
        return logs.Last();
    }

    private void UpdateQueuedLogs()
    {
        Log log;
        while (queuedLogs.TryDequeue(out log))
            ProcessLogItem(log);
    }

    private void HandleLogThreaded(string message, string stackTrace, LogType type)
    {
        var log = new Log
        {
            count = 1,
            message = message,
            stackTrace = stackTrace,
            type = type,
        };
        queuedLogs.Enqueue(log);
    }

    private void ProcessLogItem(Log log)
    {
        var lastLog = GetLastLog();
        var isDuplicateOfLastLog = lastLog.HasValue && log.Equals(lastLog.Value);

        if (isDuplicateOfLastLog)
        {
            
            log.count = lastLog.Value.count + 1;
            logs[logs.Count - 1] = log;
        }
        else
        {
            logs.Add(log);
            TrimExcessLogs();
        }
    }

    private bool IsScrolledToBottom(Rect innerScrollRect, Rect outerScrollRect)
    {
        var innerScrollHeight = innerScrollRect.height;
        var outerScrollHeight = outerScrollRect.height - GUI.skin.box.padding.vertical;

        if (outerScrollHeight > innerScrollHeight)
            return true;

        return Mathf.Approximately(innerScrollHeight, scrollPosition.y + outerScrollHeight);
    }

    private void TrimExcessLogs()
    {
        if (!restrictLogCount)
            return;

        var amountToRemove = logs.Count - maxLogCount;

        if (amountToRemove <= 0)
            return;

        logs.RemoveRange(0, amountToRemove);
    }

    private void OnDisable() => Application.logMessageReceivedThreaded -= HandleLogThreaded;
    private void OnEnable() => Application.logMessageReceivedThreaded += HandleLogThreaded;

    private bool IsLogVisible(Log log) => logTypeFilters[log.type];
    private void ScrollToBottom() => scrollPosition = new Vector2(0, Int32.MaxValue);
    private void OpenConsoleOnButtonUI() => isVisible = !isVisible;
}

struct Log
{
    public int count;
    public string message;
    public string stackTrace;
    public LogType type;

    const int maxMessageLength = 16382;

    public bool Equals(Log log)
    {
        return message == log.message && stackTrace == log.stackTrace && type == log.type;
    }

    public string GetTruncatedMessage()
    {
        if (string.IsNullOrEmpty(message))
        {
            return message;
        }
        return message.Length <= maxMessageLength ? message : message.Substring(0, maxMessageLength);
    }
}

class ConcurrentQueue<T>
{
    readonly Queue<T> queue = new Queue<T>();
    readonly object queueLock = new object();

    public void Enqueue(T item)
    {
        lock (queueLock)
        {
            queue.Enqueue(item);
        }
    }

    public bool TryDequeue(out T result)
    {
        lock (queueLock)
        {
            if (queue.Count == 0)
            {
                result = default(T);
                return false;
            }

            result = queue.Dequeue();
            return true;
        }
    }
}
