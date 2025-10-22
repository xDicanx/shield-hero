using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SH.Core;

// Halo breve que aparece sobre el héroe al parrear (escucha OnParrySuccess).
[DisallowMultipleComponent]
public class ParryPulse : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Canvas canvas;                 // Canvas contenedor
    [SerializeField] private RectTransform ring;            // Image circular del halo
    [SerializeField] private Transform worldTarget;         // Transform del héroe

    [Header("Ajustes visuales")]
    [SerializeField] private Color ringColor = new Color(0.2f, 0.9f, 1f, 1f);
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float startScale = 0.65f;
    [SerializeField] private float endScale = 1.25f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    CanvasGroup cg;
    Image ringImage;
    Coroutine playing;

    void Reset()
    {
        canvas = GetComponentInParent<Canvas>();
        if (!ring) ring = GetComponent<RectTransform>();
    }

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!ring) ring = GetComponent<RectTransform>();
        if (ring)
        {
            cg = ring.GetComponent<CanvasGroup>();
            if (!cg) cg = ring.gameObject.AddComponent<CanvasGroup>();
            ringImage = ring.GetComponent<Image>();
            if (!ringImage) ringImage = ring.gameObject.AddComponent<Image>();
            ringImage.raycastTarget = false;
        }
        SetVisible(false);
    }

    void OnEnable()
    {
        CombatEvents.OnParrySuccess += HandleParry;
    }

    void OnDisable()
    {
        CombatEvents.OnParrySuccess -= HandleParry;
    }

    void HandleParry(IActor source, IActor target)
    {
        Pulse();
    }

    public void SetTarget(Transform t) => worldTarget = t;

    public void Pulse()
    {
        if (!ring) return;
        if (playing != null) StopCoroutine(playing);
        playing = StartCoroutine(PulseCo());
    }

    IEnumerator PulseCo()
    {
        // Colocar el halo sobre el héroe en el Canvas
        if (canvas && worldTarget && Camera.main)
        {
            Vector2 sp = Camera.main.WorldToScreenPoint(worldTarget.position);
            var canvasRT = canvas.transform as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRT, sp,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    out var lp))
            {
                ring.anchoredPosition = lp;
            }
        }

        if (ringImage) ringImage.color = ringColor;
        SetVisible(true);
        ring.localScale = Vector3.one * startScale;
        cg.alpha = 1f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, duration);
            float e = ease != null ? ease.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);
            ring.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, e);
            cg.alpha = 1f - e;
            yield return null;
        }

        SetVisible(false);
        playing = null;
    }

    void SetVisible(bool v)
    {
        if (cg) cg.alpha = v ? 1f : 0f;
        if (ring) ring.gameObject.SetActive(v);
    }
}