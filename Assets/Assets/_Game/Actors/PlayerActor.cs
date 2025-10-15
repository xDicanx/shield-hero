
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

        public override void TakeTurn(System.Action<ActionData> onActionReady)
        {
            bool canShield = shield && shield.Charges > 0;
            string extra = canShield ? "  S=Shield" : "";
            Debug.Log($"-- TURNO de {Name} -- (A=Attack  D=Defend  W=Wait{extra})");

            loop.WaitForPlayerInput(keys =>
            {
                var target = loop.AliveActors().FirstOrDefault(a => a.Team == Team.Enemy);

                if (keys.attack)
                {
                    if (target == null) { onActionReady(new ActionData(ActionType.Wait, this, null)); return; }
                    int dmg = this.Attack;
                    if (shield) dmg += shield.ConsumeForBonus(); // consume si hubiera
                    onActionReady(new ActionData(ActionType.Attack, this, target, dmg));
                }
                else if (keys.shield && canShield)
                {
                    if (target == null) { onActionReady(new ActionData(ActionType.Wait, this, null)); return; }
                    // ShieldSkill: consume TODAS las cargas para un golpe de escudo
                    int bonus = shield.ConsumeForBonus();         // consume (cargas * damagePerCharge)
                    int dmg   = Mathf.Max(1, bonus);              // puro daño por cargas
                    onActionReady(new ActionData(ActionType.ShieldSkill, this, target, dmg));
                }
                else if (keys.defend)
                {
                    onActionReady(new ActionData(ActionType.Defend, this, null));
                }
                else
                {
                    onActionReady(new ActionData(ActionType.Wait, this, null));
                }
            });
        }
    }
}
