using System;
using System.Collections.Generic;
using UniRx;
using UnityEditor;
using UnityEngine;

public class LauncherManager : Singleton<LauncherManager>
{
    private static bool inited;

    public static bool IsAppLaunching => !inited;

    [SerializeField] private List<Component> globalAwakes = new List<Component>();
    [SerializeField] private List<Component> awakes = new List<Component>();
    [SerializeField] private List<Component> starts = new List<Component>();
    
    private List<ILauncherGlobalAwake> iGlobalAwakes = new List<ILauncherGlobalAwake>();
    private List<ILauncherAwake> iAwakes = new List<ILauncherAwake>();
    private List<ILauncherStart> iStarts = new List<ILauncherStart>();

    protected override void AwakeSingletone()
    {
        globalAwakes.ForEach(x => iGlobalAwakes.Add(x as ILauncherGlobalAwake));
        awakes.ForEach(x => iAwakes.Add(x as ILauncherAwake));
        starts.ForEach(x => iStarts.Add(x as ILauncherStart));
        
        if (IsAppLaunching)
            iGlobalAwakes.ForEach(x => x.AwakeGlobal());
        
        iAwakes.ForEach(x => x.AwakeLauncher());
        Observable.TimerFrame(2).Subscribe(x => inited = true);
    }

    private void Start()
    {
        iStarts.ForEach(x => x.StartLauncher());
    }

#if UNITY_EDITOR
    public void FindAllComponents()
    {
        List<GameObject> objectsInScene = new List<GameObject>();

        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                objectsInScene.Add(go);
        }

        globalAwakes.Clear();
        awakes.Clear();
        starts.Clear();

        foreach (GameObject go in objectsInScene)
        {
            if (go.TryGetComponent(out ILauncherGlobalAwake globalAwake))
                globalAwakes.Add(globalAwake as Component);
            
            if (go.TryGetComponent(out ILauncherAwake awake))
                awakes.Add(awake as Component);
            
            if (go.TryGetComponent(out ILauncherStart start))
                starts.Add(start as Component);
        }
    }
#endif
}