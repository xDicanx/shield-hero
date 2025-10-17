using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.Actors
{
    [DisallowMultipleComponent]
    public class SpriteFlash : MonoBehaviour
    {
        [Header("Defaults")]
        [SerializeField] private Color flashColor = new Color(1f, 0.92f, 0.2f, 1f); // amarillo cálido
        [SerializeField, Min(0f)] private float flashDuration = 0.12f;
        [Tooltip("Si está activo, recaptura los colores base justo antes de cada flash. Úsalo solo si otros sistemas alteran el color base fuera de este efecto.")]
        [SerializeField] private bool recaptureOnEachFlash = false; // por defecto OFF para evitar capturar colores ya tinteados

        // 2D
        private SpriteRenderer[] _spriteRenderers;
        private Color[] _baseSpriteColors;

        // Para SpriteRenderers con material no "Sprites/*" (p.ej., URP Unlit)
        private bool[] _useMatTintForSR;
        private Color[] _baseSpriteMatColors;

        // 3D (Mesh/SkinnedMesh)
        private Renderer[] _renderers3D;
        private Color[] _baseMaterialColors;

        private Coroutine _flashCo;

        void Awake()
        {
            CacheRenderers();
            CaptureBaseColors();
        }

        void OnEnable()  => RestoreBaseColors();
        void OnDisable() { StopFlashIfRunning(); RestoreBaseColors(); }
        void OnDestroy() { StopFlashIfRunning(); RestoreBaseColors(); }

        public void Flash()               => Flash(flashDuration, flashColor);
        public void Flash(float duration) => Flash(duration, flashColor);
        public void Flash(Color color)    => Flash(flashDuration, color);

        public void Flash(float duration, Color color)
        {
            if (duration <= 0f) duration = 0.01f;

            if (NoTargetsCached())
                CacheRenderers();

            if (NoTargetsCached())
                return;

            // CAMBIO CLAVE: ante un nuevo flash, primero garantizar que estamos en el color base
            StopFlashIfRunning();
            RestoreBaseColors();

            // Solo después, si realmente quieres capturar el estado actual como “base”, hazlo aquí.
            if (recaptureOnEachFlash)
                CaptureBaseColors();

            _flashCo = StartCoroutine(CoFlash(duration, color));
        }

        public void RefreshRenderers()
        {
            CacheRenderers();
            CaptureBaseColors();
        }

        private bool NoTargetsCached()
        {
            bool no2D = (_spriteRenderers == null || _spriteRenderers.Length == 0);
            bool no3D = (_renderers3D == null || _renderers3D.Length == 0);
            return no2D && no3D;
        }

        private void CacheRenderers()
        {
            // 2D
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true) ?? new SpriteRenderer[0];
            int nSR = _spriteRenderers.Length;
            _baseSpriteColors    = new Color[nSR];
            _useMatTintForSR     = new bool[nSR];
            _baseSpriteMatColors = new Color[nSR];

            for (int i = 0; i < nSR; i++)
            {
                var sr = _spriteRenderers[i];
                var mat = sr ? sr.sharedMaterial : null;
                bool notSpriteShader = mat != null && mat.shader != null && !mat.shader.name.StartsWith("Sprites/");
                bool hasColorProp    = mat && (mat.HasProperty("_BaseColor") || mat.HasProperty("_Color"));
                _useMatTintForSR[i]  = notSpriteShader && hasColorProp;
            }

            // 3D: todos los Renderer excepto SpriteRenderer
            var allRenderers = GetComponentsInChildren<Renderer>(includeInactive: true) ?? new Renderer[0];
            var list3D = new List<Renderer>(allRenderers.Length);
            foreach (var r in allRenderers)
            {
                if (r is SpriteRenderer) continue;
                if (r != null) list3D.Add(r);
            }
            _renderers3D = list3D.ToArray();
            _baseMaterialColors = new Color[_renderers3D.Length];
        }

        private void CaptureBaseColors()
        {
            // 2D base
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                var sr = _spriteRenderers[i];
                _baseSpriteColors[i] = sr ? sr.color : Color.white;

                if (_useMatTintForSR[i] && sr)
                {
                    var mat = sr.material; // instancia segura
                    _baseSpriteMatColors[i] = ReadMaterialColor(mat);
                }
                else
                {
                    _baseSpriteMatColors[i] = Color.white; // no se usa para este índice
                }
            }

            // 3D base
            for (int i = 0; i < _renderers3D.Length; i++)
            {
                var r = _renderers3D[i];
                _baseMaterialColors[i] = ReadMaterialColor(r ? r.material : null);
            }
        }

        private void RestoreBaseColors()
        {
            // 2D
            int nSR = Mathf.Min(_spriteRenderers?.Length ?? 0, _baseSpriteColors?.Length ?? 0);
            for (int i = 0; i < nSR; i++)
            {
                var sr = _spriteRenderers[i];
                if (!sr) continue;

                if (_useMatTintForSR[i])
                {
                    var mat = sr.material;
                    WriteMaterialColor(mat, _baseSpriteMatColors[i]);
                }
                else
                {
                    sr.color = _baseSpriteColors[i];
                }
            }

            // 3D
            int n3D = Mathf.Min(_renderers3D?.Length ?? 0, _baseMaterialColors?.Length ?? 0);
            for (int i = 0; i < n3D; i++)
            {
                WriteMaterialColor(_renderers3D[i] ? _renderers3D[i].material : null, _baseMaterialColors[i]);
            }
        }

        private void StopFlashIfRunning()
        {
            if (_flashCo != null)
            {
                StopCoroutine(_flashCo);
                _flashCo = null;
            }
        }

        private IEnumerator CoFlash(float duration, Color targetColor)
        {
            float half = duration * 0.5f;

            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / half);
                LerpAll(toTarget: true, k, targetColor);
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / half);
                LerpAll(toTarget: false, k, targetColor);
                yield return null;
            }

            RestoreBaseColors();
            _flashCo = null;
        }

        private void LerpAll(bool toTarget, float k, Color target)
        {
            // 2D
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                var sr = _spriteRenderers[i];
                if (!sr) continue;

                if (_useMatTintForSR[i])
                {
                    var mat = sr.material;
                    var baseCol = _baseSpriteMatColors[i];
                    var col = toTarget ? Color.Lerp(baseCol, target, k) : Color.Lerp(target, baseCol, k);
                    WriteMaterialColor(mat, col);
                }
                else
                {
                    var baseCol = _baseSpriteColors[i];
                    sr.color = toTarget ? Color.Lerp(baseCol, target, k)
                                        : Color.Lerp(target, baseCol, k);
                }
            }

            // 3D
            for (int i = 0; i < _renderers3D.Length; i++)
            {
                var r = _renderers3D[i];
                if (!r) continue;
                var baseCol = _baseMaterialColors[i];
                var col = toTarget ? Color.Lerp(baseCol, target, k) : Color.Lerp(target, baseCol, k);
                WriteMaterialColor(r.material, col);
            }
        }

        // Helpers materiales (URP Lit/Unlit: _BaseColor; otros: _Color)
        private static Color ReadMaterialColor(Material m)
        {
            if (!m) return Color.white;
            if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor");
            if (m.HasProperty("_Color"))     return m.GetColor("_Color");
            try { return m.color; } catch { return Color.white; }
        }

        private static void WriteMaterialColor(Material m, Color c)
        {
            if (!m) return;
            if (m.HasProperty("_BaseColor")) { m.SetColor("_BaseColor", c); return; }
            if (m.HasProperty("_Color"))     { m.SetColor("_Color", c);     return; }
            try { m.color = c; } catch { /* ignorar */ }
        }

        // APIs estáticas
        public static void TryFlashOn(GameObject go, float duration = 0.12f)
        {
            if (!go) return;
            var flash = go.GetComponent<SpriteFlash>() ?? go.AddComponent<SpriteFlash>();
            flash.Flash(duration);
        }

        public static void TryFlashOn(GameObject go, float duration, Color color)
        {
            if (!go) return;
            var flash = go.GetComponent<SpriteFlash>() ?? go.AddComponent<SpriteFlash>();
            flash.Flash(duration, color);
        }
    }
}