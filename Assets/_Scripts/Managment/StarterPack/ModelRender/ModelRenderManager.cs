using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;

public class ModelRenderManager : Singleton<ModelRenderManager>
{
    [SerializeField] private Transform modelRenderUnitsParent;
    [SerializeField] private GameObject modelRenderUnitPrefab;
    [SerializeField] private float unitsOffset;
    
    private Dictionary<string, RenderUnit> registeredModels = new Dictionary<string, RenderUnit>();
    
    public RenderUnit RegisterModel(string key, GameObject modelPrefab, Vector3 offset, Quaternion rotation, Vector3 size, Vector2Int renderSize, bool renderOnce = false)
    {
        GameObject renderUnit = Instantiate(modelRenderUnitPrefab, modelRenderUnitsParent, true);
        renderUnit.name = key;
        Camera camera = renderUnit.transform.GetComponentInChildren<Camera>(true); 
        renderUnit.transform.localPosition = new Vector3(0, registeredModels.Count * unitsOffset, 0);
        RenderTexture texture = RenderTexture.GetTemporary(renderSize.x, renderSize.y);
        texture.depth = 24;
        camera.targetTexture = texture;
        camera.cullingMask = int.MaxValue;

        GameObject model = Instantiate(modelPrefab, renderUnit.transform, true);
        model.transform.localPosition = offset;
        model.transform.rotation = rotation;
        model.transform.localScale = size;
        
        RenderUnit renderUnitComponent = renderUnit.GetComponent<RenderUnit>();
        renderUnitComponent.Initialize(texture, model);
        
        if (renderOnce)
            Observable.TimerFrame(2).Subscribe(x => { renderUnitComponent.ToggleRender(false); });
        
        registeredModels.Add(key, renderUnitComponent);
        return renderUnitComponent;
    }

    public void UnRegisterModel(RenderUnit unit)
    {
        registeredModels.Remove(unit.name);
        unit.RenderTexture.Release();
        Destroy(unit.RenderTexture);
        Destroy(unit.gameObject);
    }

    [ContextMenu("Capture Render")]
    public void CaptureRender()
    {
        foreach (KeyValuePair<string, RenderUnit> pair in registeredModels)
        {
            Camera camera = modelRenderUnitsParent.Find(pair.Key).Find("Camera").GetComponent<Camera>();
            RenderTexture activeRenderTexture = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;
 
            camera.Render();
 
            Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
            image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            image.Apply();
            RenderTexture.active = activeRenderTexture;
 
            byte[] bytes = image.EncodeToPNG();
            Destroy(image);
 
            File.WriteAllBytes(Application.dataPath + $"/Data/Render/{pair.Key}.png", bytes);
        }
    }

    public RenderUnit GetModel(string key)
    {
        return registeredModels.Get(key);
    }
}