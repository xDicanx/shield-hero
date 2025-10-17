using UnityEngine;
using SH.Actors;

public class SpriteFlashKeyTester : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            SpriteFlash.TryFlashOn(gameObject, 0.12f); // amarillo

        if (Input.GetKeyDown(KeyCode.G))
            SpriteFlash.TryFlashOn(gameObject, 0.10f, new Color(0.2f, 0.9f, 1f, 1f)); // cian
    }
}