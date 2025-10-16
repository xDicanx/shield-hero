using System.Collections.Generic;
using UnityEngine;
using SH.Core;

/// <summary>
/// Inicializa y valida la lista de actores para el ciclo de turnos.
/// </summary>
namespace SH.Core
{
    public class TurnSetup
    {
        /// <summary>
        /// Prepara el orden de turnos a partir de referencias de MonoBehaviour.
        /// </summary>
        public List<IActor> SetupTurnOrder(List<MonoBehaviour> turnOrderRefs)
        {
            var order = new List<IActor>();
            foreach (var c in turnOrderRefs)
            {
                if (c is IActor actor && actor != null)
                    order.Add(actor);
            }
            return order;
        }
    }
}