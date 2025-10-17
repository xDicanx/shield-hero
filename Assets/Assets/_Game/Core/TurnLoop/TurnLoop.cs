using System.Collections.Generic;
using UnityEngine;
using SH.Core;
using SH.Actors;

/// <summary>
/// Orquesta el flujo principal de turnos, delegando tareas a módulos especializados.
/// </summary>
namespace SH.Core
{
    public class TurnLoop : MonoBehaviour
    {
        public IActor CurrentActor { get; private set; }
        [Header("Orden de turnos (asigna en el Inspector)")]
        public List<MonoBehaviour> turnOrderRefs;

        // Submódulos extraídos
        private TurnSetup setup;
        private TurnVictoryChecker victoryChecker;
        private TurnRunner runner;

        private List<IActor> order;
        private int index;
        private bool running;

        // Input simple
        System.Action<(bool attack, bool defend, bool shield)> onPlayerKeys;

        void Awake()
        {
            setup = new TurnSetup();
            victoryChecker = new TurnVictoryChecker();
            runner = new TurnRunner();
        }

        void Start()
        {
            order = setup.SetupTurnOrder(turnOrderRefs);

            if (order.Count == 0)
            {
                Debug.LogError("TurnLoop: sin actores asignados.");
                enabled = false; return;
            }

            StartCoroutine(MainLoop());
        }

        /// <summary>
        /// Devuelve actores vivos.
        /// </summary>
        public IEnumerable<IActor> AliveActors() => order?.FindAll(a => a.IsAlive);

        /// <summary>
        /// Loop principal que coordina turnos.
        /// </summary>
        private System.Collections.IEnumerator MainLoop()
        {
           running = true;
            while (running)
            {
                var state = victoryChecker.CheckVictoryOrDefeat(order);
                if (state != VictoryState.None)
                {
                    Debug.Log(state == VictoryState.PlayersWin ? "VICTORY!" : "DEFEAT...");
                    yield break;
                }

                var actor = NextAliveActor();
                if (actor == null) { yield return null; continue; }

                bool done = false;
                CurrentActor = actor;
                runner.RunActorTurn(actor, action =>
                {
                    if (actor.IsAlive) ActionResolver.Resolve(action);
                    LogBoardState();
                    done = true;
                }, WaitForPlayerInput);

                // Esperar a que el actor elija acción
                while (!done) yield return null;

                // NUEVO: si hay sequencer, esperar a que termine la timeline antes de pasar al siguiente actor.
                // Fallback: si no hay sequencer reproduciendo, mantener pequeña pausa.
                var seq = SH.Core.ActionStepSequencer.Instance; // ok si auto-instancia; IsPlaying será false si no se usó
                if (seq != null)
                {
                    // Si no se inició ninguna timeline, IsPlaying ya será false y no bloquea.
                    while (seq.IsPlaying)
                        yield return null;
                }
                else
                {
                    // Fallback legacy (no debería ocurrir si el Instance auto-crea)
                    yield return new WaitForSeconds(0.4f);
                }
            }
            
        }

        /// <summary>
        /// Busca el siguiente actor vivo en orden.
        /// </summary>
        private IActor NextAliveActor()
        {
            if (order == null || order.Count == 0) return null;
            for (int i = 0; i < order.Count; i++)
            {
                index = (index + 1) % order.Count;
                if (order[index].IsAlive) return order[index];
            }
            return null;
        }

        /// <summary>
        /// Espera input del jugador y lo pasa al callback.
        /// </summary>
        public void WaitForPlayerInput(System.Action<(bool attack, bool defend, bool shield)> callback)
        {
            onPlayerKeys = callback;
        }

        /// <summary>
        /// Loguea estado del tablero.
        /// </summary>
        private void LogBoardState()
        {
            var lines = new List<string>();
            lines.Add("=== BOARD ===");
            foreach (var a in order)
            {
                string side = a.Team == Team.Player ? "P" : "E";
                string life = a.IsAlive ? $"{a.HP}/{a.MaxHP}" : "DEAD";
                lines.Add($"{side} | {a.Name,-12} | {life}");
            }
            lines.Add("=============");
            Debug.Log(string.Join("\n", lines));
        }

        void Update()
        {
            //Hook global para ventana de parry (180ms)
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Space))
                SH.Core.ParryWindow.RegisterTap();
            runner.HandlePlayerInput(ref onPlayerKeys);
        }
    }
}