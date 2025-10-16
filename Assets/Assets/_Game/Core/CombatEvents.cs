using System;

namespace SH.Core
{
    public static class CombatEvents
    {
        // Quien ataca, a quién; daño aplicado final (después de parry/defensa)
        public static Action<IActor, IActor, int> OnDamageApplied;

        // Parry exitoso: atacante (enemigo), defensor (jugador)
        public static Action<IActor, IActor> OnParrySuccess;

        // Actor murió
        public static Action<IActor> OnActorDied;
    }
}
