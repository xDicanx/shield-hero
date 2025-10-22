using UnityEngine;
using System.Linq;
using SH.Core;

namespace SH.Actors
{
    public class PlayerActor : ActorBase
    {
        [Tooltip("Referencia al TurnLoop en escena")]
        public TurnLoop loop;

        ShieldChargeSystem shield;

        protected override void Awake()
        {
            base.Awake();
            shield = GetComponent<ShieldChargeSystem>();
        }

        void OnValidate()
        {
            if (!loop) loop = FindObjectOfType<TurnLoop>();
        }

        /// <summary>
        /// Solicita input del jugador y procesa el resultado como acción.
        /// </summary>
        public override void TakeTurn(System.Action<ActionData> onActionReady)
        {
            // UI-driven: si el Panel está en UI + MenuThenTarget, delega a TurnLoop.
            if (loop != null && loop.UseMenuThenTarget)
            {
                loop.RequestPlayerActionViaUI(onActionReady);
                return;
            }

            // Legacy (teclado A/D/W/S) — comportamiento original
            bool canShield = shield && shield.Charges > 0;
            Debug.Log($"-- TURNO de {Name} -- (A=Attack  D=Defend  W=Wait{(canShield ? "  S=Shield" : "")})");

            loop.WaitForPlayerInput(keys =>
            {
                var target = loop.AliveActors().FirstOrDefault(a => a.Team == Team.Enemy);
                onActionReady(ProcessPlayerInput(keys, target, canShield));
            });
        }

        /// <summary>
        /// Construye el ActionData desde el input legacy.
        /// </summary>
        private ActionData ProcessPlayerInput((bool attack, bool defend, bool shield) keys, IActor target, bool canShield)
        {
            if (keys.attack)
            {
                if (target == null)
                    return new ActionData(ActionType.Wait, this, null);

                int dmg = this.Attack;
                if (shield)
                    dmg += shield.ConsumeForBonus();
                return new ActionData(ActionType.Attack, this, target, dmg);
            }
            else if (keys.shield && canShield)
            {
                if (target == null)
                    return new ActionData(ActionType.Wait, this, null);

                int bonus = shield.ConsumeForBonus();
                int dmg = Mathf.Max(1, bonus);
                return new ActionData(ActionType.ShieldSkill, this, target, dmg);
            }
            else if (keys.defend)
            {
                return new ActionData(ActionType.Defend, this, null);
            }
            else
            {
                return new ActionData(ActionType.Wait, this, null);
            }
        }
    }
}