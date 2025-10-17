using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SH.Core;
using SH.Actors;

namespace SH.UI
{
    // HUD compacto de cargas (0–5 orbes). Refresca en parry y en cambios detectados.
    [DisallowMultipleComponent]
    public class ShieldChargeHUD : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Contenedor de los orbes (RectTransform)")]
        [SerializeField] private RectTransform container;
        [Tooltip("Prefab de orb (Image). Si no hay pool, se instancian hasta maxOrbs.")]
        [SerializeField] private Image orbPrefab;
        [Tooltip("Actor jugador (para leer ShieldChargeSystem).")]
        [SerializeField] private PlayerActor hero;
        [Tooltip("Pulse opcional a disparar al ganar carga (Parry).")]
        [SerializeField] private ParryPulse parryPulse;

        [Header("Ajustes")]
        [Range(1, 8)] [SerializeField] private int maxOrbs = 5;
        [SerializeField] private Color onColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color offColor = new Color(1f, 1f, 1f, 0.18f);
        [Tooltip("Refrescar automáticamente cuando cambie el valor leído.")]
        [SerializeField] private bool autoPoll = true;

        readonly List<Image> orbs = new List<Image>(8);
        int lastValue = -1;

        void Reset()
        {
            container = (RectTransform)transform;
        }

        void Awake()
        {
            if (!container) container = (RectTransform)transform;
            BuildOrbsIfNeeded();
            RefreshFromHero(); // inicial
        }

        void OnEnable()
        {
            CombatEvents.OnParrySuccess += OnParry;
            var seq = ActionStepSequencer.Instance;
            if (seq != null) seq.OnStepStarted += OnStepStarted;
        }

        void OnDisable()
        {
            CombatEvents.OnParrySuccess -= OnParry;
            var seq = ActionStepSequencer.Instance;
            if (seq != null) seq.OnStepStarted -= OnStepStarted;
        }

        void Update()
        {
            if (!autoPoll) return;
            RefreshFromHero();
        }

        void BuildOrbsIfNeeded()
        {
            // Reutiliza hijos existentes si ya hay; si faltan, instancia.
            orbs.Clear();
            int existing = container.childCount;
            for (int i = 0; i < Mathf.Max(existing, maxOrbs); i++)
            {
                Image img = null;
                if (i < existing)
                {
                    img = container.GetChild(i).GetComponent<Image>();
                }
                else if (orbPrefab)
                {
                    img = Instantiate(orbPrefab, container);
                    img.name = $"Orb_{i + 1}";
                }
                else
                {
                    // Crear orb mínimo si no hay prefab
                    var go = new GameObject($"Orb_{i + 1}", typeof(RectTransform), typeof(Image));
                    go.transform.SetParent(container, false);
                    img = go.GetComponent<Image>();
                }

                if (img)
                {
                    img.color = offColor;
                    orbs.Add(img);
                }
            }
            // Limitar a maxOrbs si hay más
            if (orbs.Count > maxOrbs)
                orbs.RemoveRange(maxOrbs, orbs.Count - maxOrbs);
        }

        // Hook por evento de Parry: dispara pulse y refresca contadores.
        void OnParry(IActor source, IActor target)
        {
            // El +1 carga real ocurre en el paso "Cargas+" de la timeline.
            // Disparamos pulse inmediato y forzamos un refresh (poll cubrirá el resto).
            if (parryPulse) parryPulse.Pulse();
            // Pequeño delay para asegurar que el paso "Cargas+" ya corrió (por si OnParrySuccess se emite antes).
            Invoke(nameof(RefreshFromHero), 0.01f);
        }

        // Fallback robusto: escuchar el label "Cargas+" del sequencer.
        void OnStepStarted(ActionStepEvent e)
        {
            if (e.Label == "Cargas+")
                RefreshFromHero();
        }

        public void RefreshFromHero()
        {
            int value = 0;
            var sc = hero ? hero.GetComponent<ShieldChargeSystem>() : null;
            if (sc) value = Mathf.Clamp(sc.Charges, 0, maxOrbs);
            UpdateCharges(value);
        }

        // API pública: ajustar manualmente
        public void UpdateCharges(int value)
        {
            value = Mathf.Clamp(value, 0, maxOrbs);
            if (value == lastValue) return;
            lastValue = value;

            for (int i = 0; i < orbs.Count; i++)
            {
                var img = orbs[i];
                if (!img) continue;
                img.color = i < value ? onColor : offColor;
                img.transform.localScale = Vector3.one * (i < value ? 1.0f : 0.9f);
            }
        }

        // Utilidad para asignar héroe en runtime si no estaba
        public void SetHero(PlayerActor p)
        {
            hero = p;
            RefreshFromHero();
        }
    }
}