using UnityEngine;
using System.Collections;
using System.Linq;
using SH.Core;

namespace SH.Actors
{
    // IA del compañero: reacciona al parry del jugador y prioriza defender con HP bajo.
    public class CompanionAI : ActorBase
    {
        [Tooltip("Referencia al TurnLoop en escena")]
        public TurnLoop loop;

        [Header("IA — Probabilidades (0..1)")]
        [SerializeField, Range(0f, 1f)] float baseAttackChance = 0.60f;   // si HP >= umbral y sin parry reciente
        [SerializeField, Range(0f, 1f)] float lowHpDefendChance = 0.60f;  // si HP < umbral → Defend 60% / Attack 40%

        [Header("IA — Umbral HP")]
        [SerializeField, Range(0.05f, 0.8f)] float lowHpThresholdPct = 0.30f;

        [Header("IA — Reacción a Parry del jugador")]
        [SerializeField, Range(0.5f, 1f)] float attackChanceAfterParry = 0.80f; // sesgo de ataque para el próximo turno

        [Header("IA — Telegraph (segundos)")]
        [SerializeField] Vector2 telegraphDelayRange = new Vector2(0.30f, 0.50f);

        bool parryBiasForNextTurn;

        void OnEnable()
        {
            CombatEvents.OnParrySuccess += HandleParrySuccess;
        }

        void OnDisable()
        {
            CombatEvents.OnParrySuccess -= HandleParrySuccess;
        }

        void OnValidate()
        {
            if (!loop) loop = FindObjectOfType<TurnLoop>();
            if (telegraphDelayRange.x < 0f) telegraphDelayRange.x = 0f;
            if (telegraphDelayRange.y < telegraphDelayRange.x) telegraphDelayRange.y = telegraphDelayRange.x;
        }

        // Se dispara cuando el jugador parrea (ActionResolver emite OnParrySuccess).
        void HandleParrySuccess(IActor attacker, IActor target)
        {
            if (target != null && target.Team == Team.Player)
                parryBiasForNextTurn = true;
        }

        public override void TakeTurn(System.Action<ActionData> onActionReady)
        {
            // Elegir objetivo enemigo vivo (simple: primero encontrado)
            var target = loop != null
                ? loop.AliveActors().FirstOrDefault(a => a.Team == Team.Enemy)
                : null;

            // Decidir intención
            var chosen = ChooseAction(hasTarget: target != null);

            // Telegraph -> Execute (similar a EnemyActor)
            StartCoroutine(TelegraphThenExecute(chosen, target, onActionReady));
        }

        ActionType ChooseAction(bool hasTarget)
        {
            if (!hasTarget) return ActionType.Wait;

            float hpPct = (float)HP / Mathf.Max(1, MaxHP);

            if (hpPct < lowHpThresholdPct)
            {
                // HP bajo: Defend 60% / Attack 40%
                return Random.value < lowHpDefendChance ? ActionType.Defend : ActionType.Attack;
            }

            float attackChance = parryBiasForNextTurn
                ? Mathf.Max(baseAttackChance, attackChanceAfterParry)
                : baseAttackChance;

            return Random.value < attackChance ? ActionType.Attack : ActionType.Defend;
        }

        IEnumerator TelegraphThenExecute(ActionType type, IActor target, System.Action<ActionData> cb)
        {
            // Delay de telegraph
            float delay = Random.Range(telegraphDelayRange.x, telegraphDelayRange.y);

            // Labels de intención (Tarea 3)
            string tellLabel = type == ActionType.Attack ? "Tell: AllyAttack"
                              : type == ActionType.Defend ? "Tell: AllyDefend"
                              : "Tell: Wait";

            // Secuencia: “Tell: X” (instantáneo) + “Banner: Intención” (delay visible)
            var steps = new System.Collections.Generic.List<ActionStep>
            {
                ActionStep.Do(tellLabel),
                ActionStep.Wait("Banner: Intención", delay),
            };
            ActionStepSequencer.PlayNow(steps);

            // Esperar a que termine el telegraph antes de ejecutar acción
            var seq = ActionStepSequencer.Instance;
            while (seq != null && seq.IsPlaying)
                yield return null;

            // Construir ActionData final (usa constructor existente en el repo)
            ActionData final;
            switch (type)
            {
                case ActionType.Attack:
                    if (target != null && target.IsAlive)
                        final = new ActionData(ActionType.Attack, this, target, this.Attack);
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

            // Consumir sesgo de parry (solo aplica al siguiente turno)
            parryBiasForNextTurn = false;

            // Entregar la acción al TurnLoop → ActionResolver
            cb?.Invoke(final);
        }
    }
}