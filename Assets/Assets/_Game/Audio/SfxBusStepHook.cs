using UnityEngine;
using SH.Core;

[DisallowMultipleComponent]
public class SfxBusStepHook : MonoBehaviour
{
    [Header("Mapeo de labels → SFX")]
    [SerializeField] private string hitLabel = "Impact";
    [SerializeField] private string parryLabel = "SFX";   // usado en BuildParrySteps
    [SerializeField] private string defendLabel = "Pose"; // usado en BuildDefendSteps

    void OnEnable()
    {
        var seq = ActionStepSequencer.Instance; // auto-instancia
        if (seq != null)
            seq.OnStepStarted += OnStepStarted;
    }

    void OnDisable()
    {
        var seq = ActionStepSequencer.Instance;
        if (seq != null)
            seq.OnStepStarted -= OnStepStarted;
    }

    private void OnStepStarted(ActionStepEvent e)
    {
        // Seguridad: si no hay bus, no hacemos nada.
        if (SfxBus.Instance == null) return;

        var label = e.Label;
        if (string.IsNullOrEmpty(label)) return;

        if (label == hitLabel)
        {
            SfxBus.PlayHit();
        }
        else if (label == parryLabel)
        {
            SfxBus.PlayParry();
        }
        else if (label == defendLabel)
        {
            SfxBus.PlayDefend();
        }
    }
}