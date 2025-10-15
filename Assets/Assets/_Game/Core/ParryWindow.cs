using UnityEngine;

namespace SH.Core
{
    // Ventana global y simple de parry basada en el último “tap” del jugador.
    // Si un ataque impacta y han pasado <= windowMs desde el último tap, es PARRY.
    public static class ParryWindow
    {
        static float lastTapTime = -999f;
        public const float windowMs = 180f;

        public static void RegisterTap()
        {
            lastTapTime = Time.time;
        }

        public static bool IsActiveNow()
        {
            return (Time.time - lastTapTime) <= (windowMs / 1000f);
        }
    }
}
