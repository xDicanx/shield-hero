using UnityEngine;
using SH.UI;

public class FloatingTextSpawnerBootstrap : MonoBehaviour
{
    public Canvas targetCanvas;
    public FloatingText floatingTextPrefab;

    void Awake()
    {
        // Fuerza que el singleton use este canvas y prefab
        if (FloatingTextSpawner.Instance != null)
        {
            FloatingTextSpawner.Instance.Configure(targetCanvas, floatingTextPrefab);
            Debug.Log("[FTBootstrap] Singleton configurado OK.");
        }
        else
        {
            var go = new GameObject("~FloatingTextSpawner");
            var spawner = go.AddComponent<FloatingTextSpawner>();
            spawner.Configure(targetCanvas, floatingTextPrefab);
            Debug.Log("[FTBootstrap] Singleton creado y configurado OK.");
        }
    }
}