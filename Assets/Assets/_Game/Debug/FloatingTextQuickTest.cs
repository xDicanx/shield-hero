using UnityEngine;
using SH.UI;

public class FloatingTextQuickTest : MonoBehaviour
{
    [Header("Opcional: asigna un objetivo en mundo")]
    public Transform target;
    public Color color = new Color(1f, 0.9f, 0.25f);
    public float duration = 0.75f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!target) target = transform;

            var cam = Camera.main;
            var wp = target.position + Vector3.up * 1.6f;
            Debug.Log($"[FTTest] H pressed. target={target.name} worldPos={wp} cam={(cam ? cam.name : "NULL")}");

            var inst = FloatingTextSpawner.Show(wp, "42", color, duration);
            if (inst)
            {
                var rt = inst.transform as RectTransform;
                Debug.Log($"[FTTest] Spawned (world) anchoredPos={rt.anchoredPosition}");
            }
            else
            {
                Debug.LogWarning("[FTTest] Spawn returned null (¿prefab sin asignar en el spawner activo?).");
            }
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            // Spawn al centro de pantalla (prueba de visibilidad)
            var cam = Camera.main;
            Vector2 screen = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f + 60f); // 60px arriba del centro

            // Convertimos un screen point a mundo para pasarle al Show (que espera worldPos)
            Vector3 world = Vector3.zero;
            if (cam)
            {
                // Profundidad: distancia desde la cámara para hacer la conversión; usa algo cercano a tu plano de juego
                float depth = 2f;
                world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
            }
            else
            {
                // Si no hay MainCamera, el spawner igualmente acepta worldPos pero quedará casi en (0,0) ∴ debería verse
                world = new Vector3(0f, 0f, 0f);
            }

            Debug.Log($"[FTTest] J pressed. screen={screen} → world={world} cam={(cam ? cam.name : "NULL")}");

            var inst = FloatingTextSpawner.Show(world, "42", Color.white, 0.9f);
            if (inst)
            {
                var rt = inst.transform as RectTransform;
                Debug.Log($"[FTTest] Spawned (center) anchoredPos={rt.anchoredPosition}");
            }
            else
            {
                Debug.LogWarning("[FTTest] Spawn (center) returned null (¿prefab sin asignar en el spawner activo?).");
            }
        }
    }
}