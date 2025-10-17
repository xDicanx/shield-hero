using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SH.Core;

namespace SH.Actors
{
    public class EnemyActor : ActorBase
    {
        [Tooltip("Referencia al TurnLoop en escena")]
        public TurnLoop loop;

        [Header("IA — Probabilidades (0..1)")]
        [SerializeField, Range(0f, 1f)] float attackProbability = 0.70f;
        [SerializeField, Range(0f, 1f)] float defendProbability = 0.20f; // Wait se deriva (1 - attack - defend)

        [Header("IA — Telegraph (segundos)")]
        [SerializeField] Vector2 telegraphDelayRange = new Vector2(0.20f, 0.30f);

        // Cooldown: no defender 2 turnos seguidos
        bool defendedLastTurn = false;

        void OnValidate()
        {
            if (!loop) loop = FindObjectOfType<TurnLoop>();
            if (telegraphDelayRange.x < 0f) telegraphDelayRange.x = 0f;
            if (telegraphDelayRange.y < telegraphDelayRange.x) telegraphDelayRange.y = telegraphDelayRange.x;
        }

        public override void TakeTurn(System.Action<ActionData> onActionReady)
        {
            // Elegir objetivo jugador vivo (simple: primero que encontremos)
            var target = loop != null
                ? loop.AliveActors().FirstOrDefault(a => a.Team == Team.Player)
                : null;

            // FSM: ChooseAction
            var chosen = ChooseAction(canDefend: !defendedLastTurn, hasTarget: target != null);

            // FSM: Telegraph -> Execute
            StartCoroutine(TelegraphThenExecute(chosen, target, onActionReady));
        }

        ActionType ChooseAction(bool canDefend, bool hasTarget)
        {
            if (!hasTarget)
                return ActionType.Wait;

            float atk = Mathf.Max(0f, attackProbability);
            float def = canDefend ? Mathf.Max(0f, defendProbability) : 0f;
            float wait = Mathf.Max(0f, 1f - atk - def);

            float sum = atk + def + wait;
            if (sum <= 0f) return ActionType.Wait;

            float r = Random.value * sum;
            if (r < atk) return ActionType.Attack;
            if (r < atk + def) return ActionType.Defend;
            return ActionType.Wait;
        }

        IEnumerator TelegraphThenExecute(ActionType type, IActor target, System.Action<ActionData> cb)
        {
            // Delay de telegraph
            float delay = Random.Range(telegraphDelayRange.x, telegraphDelayRange.y);

            // Labels de intención
            string tellLabel = type == ActionType.Attack ? "Tell: Attack"
                              : type == ActionType.Defend ? "Tell: Defend"
                              : "Tell: Wait";

            // Secuencia de Telegraph: "Tell: X" (instantáneo) + "Banner: Intención" (delay visible)
            var steps = new List<ActionStep>
            {
                ActionStep.Do(tellLabel),
                ActionStep.Wait("Banner: Intención", delay),
            };

            ActionStepSequencer.PlayNow(steps);

            // Esperar a que termine el telegraph local antes de ejecutar acción
            var seq = ActionStepSequencer.Instance;
            while (seq != null && seq.IsPlaying)
                yield return null;

            // Construir ActionData final
            ActionData final;
            switch (type)
            {
                case ActionType.Attack:
                    if (target != null && target.IsAlive)
                        final = new ActionData(ActionType.Attack, this, target, Attack);
                    else
                        final = new ActionData(ActionType.Wait, this, null);
                    break;
                case ActionType.Defend:
                    final = new ActionData(ActionType.Defend, this, null);
                    break;
                default:
                    final = new ActionData(ActionType.Wait, this, null);
                    break;
            }

            // Actualizar cooldown
            defendedLastTurn = (final.Type == ActionType.Defend);

            // Entregar acción para resolución (ActionResolver + timelines propias)
            cb?.Invoke(final);
        }
    }
}