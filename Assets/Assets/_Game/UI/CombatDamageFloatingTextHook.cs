using UnityEngine;
using SH.Core;
using SH.UI;

// Hook UI: muestra daño flotante al recibir daño, escuchando evento global
public class CombatDamageFloatingTextHook : MonoBehaviour
{
    [Header("Opcional")]
    public Color damageColor = new Color(1f, 0.95f, 0.2f);
    public float floatDuration = 0.75f;
    public Vector3 worldOffset = new Vector3(0f, 1.6f, 0f);

    void Awake()
    {
        CombatEvents.OnDamageApplied += OnDamageApplied;
        CombatEvents.OnParrySuccess += OnParrySuccess;
    }

    void OnDestroy()
    {
        CombatEvents.OnDamageApplied -= OnDamageApplied;
        CombatEvents.OnParrySuccess -= OnParrySuccess;
    }

    private void OnDamageApplied(IActor source, IActor target, int amount)
    {
        Debug.Log($"[CombatDamageFloatingTextHook] OnDamageApplied: source={source?.Name}, target={target?.Name}, amount={amount}");
        // Solo mostrar si el daño es positivo y el target tiene Transform
        if (target?.Transform == null || amount <= 0) return;

        var pos = target.Transform.position + worldOffset;
        FloatingTextSpawner.Show(pos, amount.ToString(), damageColor, floatDuration);
        
    }

    private void OnParrySuccess(IActor source, IActor target)
    {
        var pos = target.Transform.position + worldOffset;
        FloatingTextSpawner.Show(pos, "PARRY", damageColor, floatDuration);
    }
}