using UnityEngine;
using TMPro;

namespace SH.UI
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class FloatingText : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Anim")]
        [SerializeField, Min(0.1f)] private float defaultDuration = 0.75f;
        [SerializeField] private float pixelsUp = 48f;
        [SerializeField] private AnimationCurve alphaOverTime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private AnimationCurve scaleOverTime = new AnimationCurve(
            new Keyframe(0f, 0.9f, 0.0f, 6.0f),
            new Keyframe(0.15f, 1.1f, 0.0f, 0.0f),
            new Keyframe(1f, 1f, -2.0f, 0.0f)
        );
        [SerializeField] private Vector2 randomJitter = new Vector2(8f, 4f);

        private RectTransform rt;

        private void Reset()
        {
            label = GetComponentInChildren<TextMeshProUGUI>(true);
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Awake()
        {
            rt = transform as RectTransform;
            if (label == null) label = GetComponentInChildren<TextMeshProUGUI>(true);
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void Play(string text, Color color, float? durationOverride = null, float? pixelsUpOverride = null)
        {
            if (label != null)
            {
                label.text = text;
                label.color = color;
            }
            StopAllCoroutines();
            StartCoroutine(Animate(durationOverride ?? defaultDuration, pixelsUpOverride ?? pixelsUp));
        }

        private System.Collections.IEnumerator Animate(float duration, float pxUp)
        {
            canvasGroup.alpha = 1f;

            var start = rt.anchoredPosition + new Vector2(
                Random.Range(-randomJitter.x, randomJitter.x),
                Random.Range(0f, randomJitter.y)
            );
            var end = start + Vector2.up * pxUp;
            rt.anchoredPosition = start;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / duration);

                rt.anchoredPosition = Vector2.LerpUnclamped(start, end, n);
                rt.localScale = Vector3.one * scaleOverTime.Evaluate(n);
                canvasGroup.alpha = alphaOverTime.Evaluate(n);

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}