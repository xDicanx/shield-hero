using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.Core
{
    /// <summary>
    /// Evento del sequencer para debug/UI.
    /// </summary>
    public struct ActionStepEvent
    {
        public string Label;
        public int Index;
        public int Total;
        public ActionStepEvent(string label, int index, int total)
        {
            Label = label; Index = index; Total = total;
        }
        public override string ToString() => $"{Index + 1}/{Total} {Label}";
    }

    /// <summary>
    /// Paso atómico de una acción: etiqueta, duración y callback opcional.
    /// </summary>
    // ... cabeceras y namespace iguales ...
    [Serializable]
    public struct ActionStep
    {
        public string Label;
        public float Duration;
        public Action Callback;

        public ActionStep(string label, float duration, Action callback = null)
        {
            Label = label;
            Duration = Mathf.Max(0f, duration);
            Callback = callback;
        }

        // Helpers existentes
        public static ActionStep Do(string label, Action cb) => new ActionStep(label, 0f, cb);
        public static ActionStep Wait(string label, float seconds) => new ActionStep(label, seconds, null);
        public static ActionStep DoWait(string label, float seconds, Action cb) => new ActionStep(label, seconds, cb);

        // NUEVO overload: permite pasos sin callback (usado por el driver)
        public static ActionStep Do(string label) => new ActionStep(label, 0f, null);
    }
// ... resto del archivo igual ...

    /// <summary>
    /// Ejecuta una secuencia de pasos de acción de forma secuencial usando Coroutine.
    /// No impone lógica de combate; solo orquesta tiempos y callbacks.
    /// </summary>
    public class ActionStepSequencer : MonoBehaviour
    {
        // Singleton liviano para facilitar uso sin cableado previo.
        static ActionStepSequencer _instance;
        public static ActionStepSequencer Instance
        {
            get
            {
                if (_instance == null)
                {
                    var found = FindObjectOfType<ActionStepSequencer>();
                    if (found != null) _instance = found;
                    else
                    {
                        var go = new GameObject("ActionStepSequencer");
                        _instance = go.AddComponent<ActionStepSequencer>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        // Eventos para overlays/herramientas
        public event Action<ActionStepEvent> OnStepStarted;
        public event Action<ActionStepEvent> OnStepEnded;

        [Range(0.2f, 2f)]
        [Tooltip("Factor global de reproducción (1 = tiempo real).")]
        public float playbackScale = 1f;

        public bool IsPlaying { get; private set; }
        public string CurrentLabel { get; private set; }

        Coroutine current;
        int currentIndex = -1;
        int total = 0;

        /// <summary>
        /// API estática de conveniencia.
        /// </summary>
        public static Coroutine PlayNow(IEnumerable<ActionStep> steps) => Instance.Play(steps);

        /// <summary>
        /// Ejecuta los pasos en orden: Start -> Callback -> Espera(duration * playbackScale) -> End.
        /// </summary>
        public Coroutine Play(IEnumerable<ActionStep> steps)
        {
            if (steps == null) return null;
            var buffer = (steps as List<ActionStep>) ?? new List<ActionStep>(steps);
            if (buffer.Count == 0) return null;

            StopCurrent();
            current = StartCoroutine(PlayRoutine(buffer));
            return current;
        }

        public void StopCurrent()
        {
            if (current != null)
            {
                StopCoroutine(current);
                current = null;
                IsPlaying = false;
                CurrentLabel = null;
                currentIndex = -1;
                total = 0;
            }
        }

        IEnumerator PlayRoutine(List<ActionStep> steps)
        {
            IsPlaying = true;
            total = steps.Count;
            currentIndex = -1;

            for (int i = 0; i < steps.Count; i++)
            {
                currentIndex = i;
                var step = steps[i];

                CurrentLabel = step.Label ?? string.Empty;
                OnStepStarted?.Invoke(new ActionStepEvent(CurrentLabel, currentIndex, total));

                // Ejecutar callback del paso (si existe)
                try
                {
                    step.Callback?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                // Espera (si duration > 0)
                float wait = Mathf.Max(0f, step.Duration) * Mathf.Max(0.001f, playbackScale);
                if (wait > 0f)
                    yield return new WaitForSeconds(wait);

                OnStepEnded?.Invoke(new ActionStepEvent(CurrentLabel, currentIndex, total));
            }

            IsPlaying = false;
            current = null;
            CurrentLabel = null;
            currentIndex = -1;
            total = 0;
        }
    }
}