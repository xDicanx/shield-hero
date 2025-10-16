using UnityEngine;
using SH.Core;
namespace SH.Core
{
    public enum Team { Player, Enemy }

    /// <summary>
    /// Tipos de acciones posibles en el sistema de combate.
    /// Agrupados por funcionalidad para mayor claridad y mantenibilidad.
    /// </summary>
    public enum ActionType
    {
        // --- ACCIONES OFENSIVAS ---
        /// <summary>
        /// Ataque físico básico al objetivo.
        /// </summary>
        Attack,

        // --- ACCIONES DEFENSIVAS ---
        /// <summary>
        /// Defensa básica: reduce el daño recibido durante el turno entrante.
        /// </summary>
        Defend,

        // --- ACCIONES PASIVAS / ESPERA ---
        /// <summary>
        /// Espera: no realiza acción activa este turno.
        /// </summary>
        Wait,

        // --- HABILIDADES ESPECIALES ---
        /// <summary>
        /// Golpe de escudo: consume cargas para aplicar un ataque/efecto especial.
        /// </summary>
        ShieldSkill
    } // + ShieldSkill

    public struct ActionData
    {
        public ActionType Type;
        public IActor Attacker;
        public IActor Target;   // puede ser null en Defend/Wait
        public int Amount;      // daño/base

        public ActionData(ActionType type, IActor attacker, IActor target, int amount = 0)
        {
            Type = type; Attacker = attacker; Target = target; Amount = amount;
        }
        public override string ToString() =>
            $"[{Type}] {Attacker?.Name} -> {(Target!=null?Target.Name:"-")} ({Amount})";
    }

    public interface IActor
    {
        string Name { get; }
        Team Team { get; }
        bool IsAlive { get; }
        int HP { get; }
        int MaxHP { get; }

        /// Solicita que el actor ejecute su turno. Debe invocar el callback cuando termine.
        void TakeTurn(System.Action<ActionData> onActionReady);
        void ReceiveDamage(int amount);
        void ReceiveDefend(); // marca defensa 1 turno
        Transform Transform { get; }
    }
}