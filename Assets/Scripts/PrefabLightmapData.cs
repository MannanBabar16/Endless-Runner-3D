using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabLightmapData : MonoBehaviour
{
    [System.Serializable]
    struct RendererInfo
    {
        public Renderer     renderer;
        public int             lightmapIndex;
        public Vector4         lightmapOffsetScale;
    }

    [SerializeField]
    RendererInfo[]    m_RendererInfo;
    [SerializeField]
    Texture2D[]     m_Lightmaps;

    private bool existsAlready = false;
    private int counter = 0;
    private int[] lightmapArrayOffsetIndex;

    void Awake()
    {
        if (m_RendererInfo == null || m_RendererInfo.Length == 0) return;

        LightmapData[] existingMaps = LightmapSettings.lightmaps;
        int offset = existingMaps.Length;

        // Combine lightmaps
        LightmapData[] newLightmaps = new LightmapData[offset + m_Lightmaps.Length];
        existingMaps.CopyTo(newLightmaps, 0);

        for (int i = 0; i < m_Lightmaps.Length; i++)
        {
            LightmapData data = new LightmapData();
            data.lightmapColor = m_Lightmaps[i];
            newLightmaps[offset + i] = data;
        }

        LightmapSettings.lightmaps = newLightmaps;

        // Apply renderer info with offset
        foreach (var info in m_RendererInfo)
        {
            if (info.renderer == null) continue;
            info.renderer.lightmapIndex += offset;
            info.renderer.lightmapScaleOffset = info.lightmapOffsetScale;
        }
    }


 
    static void ApplyRendererInfo (RendererInfo[] infos, int[] arrayOffsetIndex)
    {
        for (int i=0;i<infos.Length;i++)
        {
            var info = infos[i];
            info.renderer.lightmapIndex = arrayOffsetIndex[info.lightmapIndex];
            info.renderer.lightmapScaleOffset = info.lightmapOffsetScale;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Bake Prefab Lightmaps")]
    static void GenerateLightmapInfo ()
    {
        if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
        {
            Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
            return;
        }
        UnityEditor.Lightmapping.Bake();

        PrefabLightmapData[] prefabs = FindObjectsOfType<PrefabLightmapData>();

        foreach (var instance in prefabs)
        {
            var gameObject = instance.gameObject;
            var rendererInfos = new List<RendererInfo>();
            var lightmaps = new List<Texture2D>();
         
            GenerateLightmapInfo(gameObject, rendererInfos, lightmaps);
         
            instance.m_RendererInfo = rendererInfos.ToArray();
            instance.m_Lightmaps = lightmaps.ToArray();

            var targetPrefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject) as GameObject;
            if (targetPrefab != null)
            {
                //UnityEditor.Prefab
                UnityEditor.PrefabUtility.ReplacePrefab(gameObject, targetPrefab);
            }
        }
    }

    static void GenerateLightmapInfo (GameObject root, List<RendererInfo> rendererInfos, List<Texture2D> lightmaps)
    {
        var renderers = root.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.lightmapIndex != -1)
            {
                RendererInfo info = new RendererInfo();
                info.renderer = renderer;
                info.lightmapOffsetScale = renderer.lightmapScaleOffset;

                Texture2D lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;

                info.lightmapIndex = lightmaps.IndexOf(lightmap);
                if (info.lightmapIndex == -1)
                {
                    info.lightmapIndex = lightmaps.Count;
                    lightmaps.Add(lightmap);
                }

                rendererInfos.Add(info);
            }
        }
    }
#endif

}