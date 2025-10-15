using UnityEngine;

public class BillboardYonly : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main;
        if (!cam) return;

        // Mirar hacia la cámara, bloqueando inclinación en X/Z
        Vector3 fwd = cam.transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude > 1e-6f)
            transform.forward = fwd;
    }
}
