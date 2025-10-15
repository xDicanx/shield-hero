using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SH.Core;
using SH.Actors;




namespace SH.Core
{
    
    public class TurnLoop : MonoBehaviour
    {
        public IActor CurrentActor { get; private set; }
        [Header("Orden de turnos (asigna en el Inspector)")]
        public List<MonoBehaviour> turnOrderRefs; // arrastra PlayerActor y EnemyActor (componentes)
        List<IActor> order;
        int index;
        bool running;

        // input buffer simple
        System.Action<(bool attack, bool defend, bool shield)> onPlayerKeys;

        void Start()
        {
            // Filtra a IActor válidos
            order = turnOrderRefs
                    .Select(c => c as IActor)
                    .Where(a => a != null)
                    .ToList();

            if (order.Count == 0)
            {
                Debug.LogError("TurnLoop: sin actores asignados.");
                enabled = false; return;
            }

            StartCoroutine(RunLoop());
        }

        public IEnumerable<IActor> AliveActors() => order.Where(a => a.IsAlive);

        IEnumerator RunLoop()
        {
            running = true;
            while (running)
            {
                // todos muertos de un lado?
                bool playersAlive = AliveActors().Any(a => a.Team == Team.Player);
                bool enemiesAlive = AliveActors().Any(a => a.Team == Team.Enemy);
                if (!playersAlive || !enemiesAlive)
                {
                    Debug.Log(playersAlive ? "VICTORY!" : "DEFEAT...");
                    yield break;
                }

                var actor = NextAliveActor();
                if (actor == null) { yield return null; continue; }

                bool done = false;
                CurrentActor = actor;
                actor.TakeTurn(action =>
                {
                    if (actor.IsAlive) ActionResolver.Resolve(action);
                    LogBoardState();
                    done = true;
                });

                // Esperar a que el actor entregue su acción
                while (!done) yield return null;

                yield return new WaitForSeconds(0.4f); // pequeño ritmo
            }
        }

        IActor NextAliveActor()
        {
            for (int i = 0; i < order.Count; i++)
            {
                index = (index + 1) % order.Count;
                if (order[index].IsAlive) return order[index];
            }
            return null;
        }

        // ===== Input mínimo para el jugador =====
        public void WaitForPlayerInput(System.Action<(bool attack, bool defend, bool shield)> callback)
        {
            onPlayerKeys = callback;
        }

        void LogBoardState()
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
                // Tap global para Parry (se queda igual)
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Space))
                SH.Core.ParryWindow.RegisterTap();

            if (onPlayerKeys != null)
            {
                bool atk = Input.GetKeyDown(KeyCode.A);
                bool def = Input.GetKeyDown(KeyCode.D);
                bool sh  = Input.GetKeyDown(KeyCode.S);
                bool any = atk || def || sh || Input.GetKeyDown(KeyCode.W);

                if (any)
                {
                    var cb = onPlayerKeys;
                    onPlayerKeys = null;
                    cb((atk, def, sh));
                }
            }

        }
    }
}
