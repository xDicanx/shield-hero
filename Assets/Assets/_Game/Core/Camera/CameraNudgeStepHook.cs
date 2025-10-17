using UnityEngine;
using SH.Core; // ActionStepSequencer, ActionStepEvent

/// <summary>
/// Escucha ActionStepSequencer.Instance.OnStepStarted y aplica un nudge de cámara
/// en pasos "Impact" (ataque) y "Parry/Flash" (parry) sin tocar la lógica de combate.
/// </summary>
[DisallowMultipleComponent]
public sealed class CameraNudgeStepHook : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CameraNudge nudge;

    [Header("Attack/Impact")]
    [SerializeField] private string impactLabel = "Impact";
    [SerializeField] private Vector2 impactDir = new Vector2(-1f, 0.1f);
    [SerializeField, Min(0f)] private float impactMagnitude = 0.08f;
    [SerializeField, Min(0.01f)] private float impactDuration = 0.14f;

    [Header("Parry/Flash")]
    [Tooltip("Label de Parry. Coincidencia por prefijo y también 'Flash' por compatibilidad.")]
    [SerializeField] private string parryLabelPrefix = "Parry";
    [SerializeField] private string parryFlashLabel = "Flash";
    [SerializeField] private Vector2 parryDir = new Vector2(1f, 0f);
    [SerializeField, Min(0f)] private float parryMagnitude = 0.04f;
    [SerializeField, Min(0.01f)] private float parryDuration = 0.12f;

    private void Reset()
    {
        if (nudge == null) nudge = FindObjectOfType<CameraNudge>();
    }

    private void Awake()
    {
        if (nudge == null) nudge = FindObjectOfType<CameraNudge>();
    }

    private void OnEnable()
    {
        var seq = ActionStepSequencer.Instance; // auto-instancia si no existe
        if (seq != null)
            seq.OnStepStarted += HandleStepStarted;
    }

    private void OnDisable()
    {
        var seq = ActionStepSequencer.Instance;
        if (seq != null)
            seq.OnStepStarted -= HandleStepStarted;
    }

    private void HandleStepStarted(ActionStepEvent e)
    {
        if (nudge == null) return;

        var label = e.Label;
        if (string.IsNullOrEmpty(label)) return;

        // Impacto de ataque
        if (label == impactLabel)
        {
            nudge.Kick(impactDir, impactMagnitude, impactDuration);
            return;
        }

        // Parry: por prefijo o por label "Flash"
        if ((parryLabelPrefix.Length > 0 && label.StartsWith(parryLabelPrefix)) ||
            (!string.IsNullOrEmpty(parryFlashLabel) && label == parryFlashLabel))
        {
            nudge.Kick(parryDir, parryMagnitude, parryDuration);
        }
    }
}