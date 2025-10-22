using System.Collections.Generic;
using UnityEngine;
using SH.Core;

namespace SH.Debugging
{
    public class ActionSequencerManualDriver : MonoBehaviour
    {
        readonly List<string> log = new List<string>(32);
        string currentLabel = "-";
        float lastStartTimeScaled = 0f;
        float lastStartTimeUnscaled = 0f;

        void OnEnable()
        {
            var inst = ActionStepSequencer.Instance; // fuerza instancia
            inst.OnStepStarted += OnStepStarted;
            inst.OnStepEnded += OnStepEnded;
        }

        void OnDisable()
        {
            var inst = ActionStepSequencer.Instance;
            if (inst != null)
            {
                inst.OnStepStarted -= OnStepStarted;
                inst.OnStepEnded -= OnStepEnded;
            }
        }

        void Update()
        {
            // Triggers por teclado (varias teclas alternativas)
            if (KeyDown(KeyCode.Alpha1, KeyCode.Keypad1, KeyCode.F1))
            {
                Debug.Log("[Driver] ATTACK key pressed");
                var steps = BuildAttackSteps();
                Debug.Log($"[Driver] Playing ATTACK steps={steps.Count}");
                ActionStepSequencer.PlayNow(steps);
            }
            if (KeyDown(KeyCode.Alpha2, KeyCode.Keypad2, KeyCode.F2))
            {
                Debug.Log("[Driver] DEFEND key pressed");
                var steps = BuildDefendSteps();
                Debug.Log($"[Driver] Playing DEFEND steps={steps.Count}");
                ActionStepSequencer.PlayNow(steps);
            }
            if (KeyDown(KeyCode.Alpha3, KeyCode.Keypad3, KeyCode.F3))
            {
                Debug.Log("[Driver] PARRY key pressed");
                var steps = BuildParrySteps();
                Debug.Log($"[Driver] Playing PARRY steps={steps.Count}");
                ActionStepSequencer.PlayNow(steps);
            }

            // Cancelar
            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("[Driver] CANCEL pressed");
                ActionStepSequencer.Instance.StopCurrent();
            }

            // Escala de reproducción (soporta [ / ] y +/-)
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftBracket) || UnityEngine.Input.GetKeyDown(KeyCode.Minus))   // [
                ActionStepSequencer.Instance.playbackScale = Mathf.Max(0.1f, ActionStepSequencer.Instance.playbackScale - 0.1f);
            if (UnityEngine.Input.GetKeyDown(KeyCode.RightBracket) || UnityEngine.Input.GetKeyDown(KeyCode.Equals)) // ]
                ActionStepSequencer.Instance.playbackScale = Mathf.Min(2.0f, ActionStepSequencer.Instance.playbackScale + 0.1f);

            // Pausa global
            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
                Time.timeScale = (Mathf.Approximately(Time.timeScale, 0f) ? 1f : 0f);
        }

        bool KeyDown(params KeyCode[] keys)
        {
            foreach (var k in keys) if (UnityEngine.Input.GetKeyDown(k)) return true;
            return false;
        }

        // Timelines de ejemplo (placeholders)
        List<ActionStep> BuildAttackSteps()
        {
            return new List<ActionStep>
            {
                ActionStep.Wait("Banner", 0.10f),
                ActionStep.Wait("Windup", 0.25f),
                ActionStep.DoWait("Impact", 0.10f, () => Debug.Log("[Impact] callback")),
                ActionStep.Do("Damage"),
                ActionStep.Wait("Settle", 0.20f),
            };
        }

        List<ActionStep> BuildDefendSteps()
        {
            return new List<ActionStep>
            {
                ActionStep.Wait("Banner", 0.10f),
                ActionStep.Wait("Pose", 0.30f),
                ActionStep.Do("BuffIcon"),
                ActionStep.Wait("Settle", 0.20f),
            };
        }

        List<ActionStep> BuildParrySteps()
        {
            return new List<ActionStep>
            {
                ActionStep.Do("Flash"),
                ActionStep.Do("SFX"),
                ActionStep.Do("Cargas+"),
                ActionStep.Wait("CameraNudge", 0.08f),
            };
        }

        void OnStepStarted(ActionStepEvent e)
        {
            currentLabel = e.Label;
            lastStartTimeScaled = Time.time;
            lastStartTimeUnscaled = Time.unscaledTime;
            var line = $"START {e.Index + 1}/{e.Total} {e.Label}";
            Push(line);
            Debug.Log($"[SEQ] {line}");
        }

        void OnStepEnded(ActionStepEvent e)
        {
            float dtScaled = Time.time - lastStartTimeScaled;
            float dtUnscaled = Time.unscaledTime - lastStartTimeUnscaled;
            var line = $"END   {e.Index + 1}/{e.Total} {e.Label}  dt={dtScaled:0.000}s (unscaled {dtUnscaled:0.000}s)";
            Push(line);
            Debug.Log($"[SEQ] {line}");
            currentLabel = "-";
        }

        void Push(string line)
        {
            log.Add(line);
            if (log.Count > 12) log.RemoveAt(0);
        }

        void OnGUI()
        {
            var seq = ActionStepSequencer.Instance;
            string header = $"Sequencer: {(seq.IsPlaying ? "PLAYING" : "IDLE")}  step={currentLabel}  scale={seq.playbackScale:0.0}  timeScale={Time.timeScale:0.0}";
            GUI.Label(new Rect(10, 10, 1200, 22), header);

            for (int i = 0; i < log.Count; i++)
                GUI.Label(new Rect(10, 34 + i * 16, 1200, 20), log[i]);

            GUILayout.BeginArea(new Rect(10, 240, 500, 120), GUI.skin.box);
            GUILayout.Label("Keys: 1=Attack  2=Defend  3=Parry  C=Cancel  [ / ]=PlaybackScale  P=Pause");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("▶ Attack", GUILayout.Height(28)))
            {
                var steps = BuildAttackSteps();
                Debug.Log($"[Driver/UI] Playing ATTACK steps={steps.Count}");
                ActionStepSequencer.PlayNow(steps);
            }
            if (GUILayout.Button("▶ Defend", GUILayout.Height(28)))
            {
                var steps = BuildDefendSteps();
                Debug.Log($"[Driver/UI] Playing DEFEND steps={steps.Count}");
                ActionStepSequencer.PlayNow(steps);
            }
            if (GUILayout.Button("▶ Parry", GUILayout.Height(28)))
            {
                var steps = BuildParrySteps();
                Debug.Log($"[Driver/UI] Playing PARRY steps={steps.Count}");
                ActionStepSequencer.PlayNow(steps);
            }
            if (GUILayout.Button("■ Cancel", GUILayout.Height(28)))
            {
                Debug.Log("[Driver/UI] CANCEL");
                ActionStepSequencer.Instance.StopCurrent();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}