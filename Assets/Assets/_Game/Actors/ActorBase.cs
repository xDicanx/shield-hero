using UnityEngine;
using SH.Core;

namespace SH.Actors
{
    public class ActorBase : MonoBehaviour, IActor
    {
        [Header("Identity")]
        [SerializeField] string displayName = "Actor";
        [SerializeField] Team team = Team.Enemy;

        [Header("Stats")]
        [SerializeField] int maxHP = 20;
        [SerializeField] int attack = 5;

        bool defending; // reduce daño a la mitad (redondeo hacia arriba)
        int hp;

        public string Name => displayName;
        public Team Team => team;
        public bool IsAlive => hp > 0;
        public int HP => hp;
        public int MaxHP => maxHP;
        public int Attack => attack;
        public Transform Transform => transform;

        protected virtual void Awake()
        {
            hp = maxHP;
        }

        public virtual void TakeTurn(System.Action<ActionData> onActionReady)
        {
            // Por defecto: esperar (los hijos lo sobreescriben)
            onActionReady?.Invoke(new ActionData(ActionType.Wait, this, null));
        }

        public virtual void ReceiveDamage(int amount)
        {
            if (amount <= 0) return;
            if (defending) amount = Mathf.CeilToInt(amount * 0.5f);
            hp = Mathf.Max(0, hp - amount);
            Debug.Log($"{Name} recibe {amount} daño. HP: {hp}/{maxHP}");
            defending = false; // defensa dura solo el turno entrante

            if (hp == 0)
            {
                var r = GetComponentInChildren<Renderer>();
                if (r && r.material.HasProperty("_Color"))
                {
                    var c = r.material.color; c *= 0.4f; r.material.color = c; // “gris”
                }
                transform.localScale *= 0.9f;
                Debug.Log($"{Name} ha CAÍDO.");
            }
            CombatEvents.OnDamageApplied?.Invoke(null, this, amount);
        }

        public virtual void ReceiveDefend()
        {
            defending = true;
            Debug.Log($"{Name} se DEFENDIÓ (mitad de daño por 1 turno).");
        }
    }
}
