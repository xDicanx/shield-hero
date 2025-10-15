using UnityEngine;

namespace SH.Core
{
    // Añádelo al Héroe (o a quien pueda acumular cargas).
    public class ShieldChargeSystem : MonoBehaviour
    {
        [Range(0,5)] [SerializeField] int charges = 0;
        public int MaxCharges => 5;
        public int Charges => charges;

        // Ajusta aquí el balance del bonus por carga
        [SerializeField] int damagePerCharge = 2;

        public void AddCharge(int amount = 1)
        {
            int before = charges;
            charges = Mathf.Clamp(charges + amount, 0, MaxCharges);
            if (charges != before)
                Debug.Log($"[Shield] Carga añadida. {before}→{charges}");
        }

        public int ConsumeForBonus()
        {
            if (charges <= 0) return 0;
            int bonus = charges * damagePerCharge;
            Debug.Log($"[Shield] Consumidas {charges} cargas → +{bonus} daño");
            charges = 0;
            return bonus;
        }
    }
}
