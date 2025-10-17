using System.Collections.Generic;
using UnityEngine;
using SH.Actors; // SpriteFlash

namespace SH.Core
{
    /// Construye timelines por tipo de acción para ejecutarlas con ActionStepSequencer.
    public static class ActionTimelineBuilder
    {
        // Attack: Banner → Windup → Impact → [Parry?] → Damage → Settle
        public static List<ActionStep> BuildAttackSteps(ActionData action, int damage, bool parry, MonoBehaviour targetMB)
        {
            var steps = new List<ActionStep>();

            steps.Add(ActionStep.Wait("Banner: Attack", 0.10f));
            steps.Add(ActionStep.Wait("Windup", 0.25f));
            steps.Add(ActionStep.DoWait("Impact", 0.10f, () =>
            {
                if (targetMB && targetMB.gameObject)
                    SpriteFlash.TryFlashOn(targetMB.gameObject, 0.12f); // amarillo cálido (default)
            }));

            if (parry)
                steps.AddRange(BuildParrySteps(action, targetMB));

            steps.Add(ActionStep.Do("Damage", () =>
            {
                if (action.Target != null && action.Target.IsAlive)
                    action.Target.ReceiveDamage(Mathf.Max(0, damage));
            }));

            steps.Add(ActionStep.Wait("Settle", 0.20f));
            return steps;
        }

        // Defend: Banner → Pose(0.3) → BuffIcon → Settle(0.2)
        public static List<ActionStep> BuildDefendSteps(ActionData action)
        {
            var steps = new List<ActionStep>();
            steps.Add(ActionStep.Wait("Banner: Defend", 0.10f));
            steps.Add(ActionStep.DoWait("Pose", 0.30f, () => action.Attacker?.ReceiveDefend()));
            steps.Add(ActionStep.Do("BuffIcon", () => { /* hook UI */ }));
            steps.Add(ActionStep.Wait("Settle", 0.20f));
            return steps;
        }

        // Parry: Flash → SFX → Cargas+ → CameraNudge
        public static List<ActionStep> BuildParrySteps(ActionData action, MonoBehaviour targetMB)
        {
            var steps = new List<ActionStep>();

            // Flash cian para diferenciar de Impact
            steps.Add(ActionStep.Do("Flash", () =>
            {
                if (targetMB && targetMB.gameObject)
                    SpriteFlash.TryFlashOn(targetMB.gameObject, 0.10f, new Color(0.2f, 0.9f, 1f, 1f)); // cian
            }));

            steps.Add(ActionStep.Do("SFX", () => { /* SfxBus.PlayParry() */ }));
            steps.Add(ActionStep.Do("Cargas+", () =>
            {
                if (targetMB)
                {
                    var charge = targetMB.GetComponent<ShieldChargeSystem>();
                    if (charge) charge.AddCharge(1);
                }
            }));
            steps.Add(ActionStep.Wait("CameraNudge", 0.08f));
            return steps;
        }

        // ShieldSkill: Banner → Windup → Impact → Damage → Settle
        // Usa action.Amount como daño (ya incluye el bonus por cargas consumidas).
        public static List<ActionStep> BuildShieldSkillSteps(ActionData action)
        {
            var steps = new List<ActionStep>();

            steps.Add(ActionStep.Wait("Banner: Shield", 0.10f));
            steps.Add(ActionStep.Wait("Windup", 0.20f));
            steps.Add(ActionStep.DoWait("Impact", 0.10f, () =>
            {
                var targetMB = action.Target as MonoBehaviour;
                if (targetMB && targetMB.gameObject)
                    SpriteFlash.TryFlashOn(targetMB.gameObject, 0.12f); // mismo color que Attack por ahora
            }));

            steps.Add(ActionStep.Do("Damage", () =>
            {
                if (action.Target != null && action.Target.IsAlive)
                    action.Target.ReceiveDamage(Mathf.Max(0, action.Amount));
            }));

            steps.Add(ActionStep.Wait("Settle", 0.20f));
            return steps;
        }
    }
}