using UnityEngine;

/// <summary>
/// Nudge sutil de cámara (ida/vuelta) para impactos y parry.
/// Adjuntar al objeto de cámara (o a un GameObject con referencia a la cámara).
/// </summary>
[DisallowMultipleComponent]
public sealed class CameraNudge : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // Si es null: usa Camera.main o este transform.

    [Header("Tuning")]
    [SerializeField, Min(0f)] private float maxMagnitude = 0.4f;
    [Tooltip("Si true, el nudge no se ve afectado por Time.timeScale.")]
    [SerializeField] private bool useUnscaledTime = false;

    private Vector3 _baseLocalPos;
    private Vector3 _currentOffset;
    private Coroutine _routine;

    private void Awake()
    {
        if (target == null)
        {
            var cam = UnityEngine.Camera.main;
            target = cam != null ? cam.transform : transform;
        }
        _baseLocalPos = target.localPosition;
    }

    private void OnDisable()
    {
        StopCurrent();
        ResetPosition();
    }

    private void OnDestroy()
    {
        ResetPosition();
    }

    /// <summary>
    /// Empuja la cámara en 'dir' con fuerza 'magnitude' y duración total 'duration' (ida+vuelta).
    /// </summary>
    public void Kick(Vector2 dir, float magnitude, float duration)
    {
        if (target == null) return;
        if (duration <= 0f || magnitude <= 0f) return;

        if (dir.sqrMagnitude < 1e-4f)
            dir = new Vector2(1f, 0f); // fallback horizontal

        var mag = Mathf.Min(Mathf.Abs(magnitude), maxMagnitude);
        var offset = (Vector3)(dir.normalized * mag);

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(KickRoutine(offset, Mathf.Max(0.01f, duration)));
    }

    /// <summary>
    /// Conveniencia: busca una instancia en escena y dispara el nudge.
    /// </summary>
    public static void TryKick(Vector2 dir, float magnitude, float duration)
    {
        var instance = FindObjectOfType<CameraNudge>();
        if (instance != null) instance.Kick(dir, magnitude, duration);
    }

    private System.Collections.IEnumerator KickRoutine(Vector3 targetOffset, float duration)
    {
        float half = duration * 0.5f;
        float t = 0f;
        var startOffset = _currentOffset;

        // Ida (ease-out)
        while (t < half)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = Mathf.Clamp01(t / half);
            float e = 1f - Mathf.Pow(1f - u, 2f); // easeOutQuad
            _currentOffset = Vector3.LerpUnclamped(startOffset, targetOffset, e);
            target.localPosition = _baseLocalPos + _currentOffset;
            yield return null;
        }

        // Vuelta (ease-in)
        t = 0f;
        startOffset = _currentOffset;
        while (t < half)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = Mathf.Clamp01(t / half);
            float e = u * u; // easeInQuad
            _currentOffset = Vector3.LerpUnclamped(startOffset, Vector3.zero, e);
            target.localPosition = _baseLocalPos + _currentOffset;
            yield return null;
        }

        _currentOffset = Vector3.zero;
        target.localPosition = _baseLocalPos;
        _routine = null;
    }

    private void ResetPosition()
    {
        if (target != null) target.localPosition = _baseLocalPos;
        _currentOffset = Vector3.zero;
    }

    private void StopCurrent()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }
}