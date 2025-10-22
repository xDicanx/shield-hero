using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SH.Core;
using SH.UI;

namespace SH.Debugging
{
    // Driver de sandbox: recolecta actores desde la escena según filtro y abre el selector.
    // Imprime la selección en consola.
    public class TargetSelectorSandboxDriver : MonoBehaviour
    {
        public TargetSelectorUI selector;
        public TargetSelectorConfig configOverride; // opcional para probar otro filtro

        [Header("Criterio de recolección")]
        public Team referenceTeam = Team.Player; // "jugador" como referencia
        public TargetFilter filter = TargetFilter.EnemiesOnly;

        void Reset()
        {
            if (!selector) selector = FindObjectOfType<TargetSelectorUI>();
        }

        void OnEnable()
        {
            if (selector) selector.OnTargetChosen += HandleChosen;
        }

        void OnDisable()
        {
            if (selector) selector.OnTargetChosen -= HandleChosen;
        }

        void Start()
        {
            if (!selector) return;

            if (configOverride)
            {
                selector.SetFilter(configOverride.filter);
            }
            else
            {
                selector.SetFilter(filter);
            }

            var candidates = CollectCandidates();
            selector.Open(candidates);
        }

        IEnumerable<IActor> CollectCandidates()
        {
            var all = new List<IActor>();
            foreach (var mb in FindObjectsOfType<MonoBehaviour>())
            {
                if (mb is IActor a && a.IsAlive)
                    all.Add(a);
            }

            bool wantEnemies = (filter == TargetFilter.EnemiesOnly);
            bool wantAllies = (filter == TargetFilter.AlliesOnly);
            bool wantAny = (filter == TargetFilter.AnyAlive);

            return all.Where(a =>
            {
                if (wantAny) return true;
                if (wantEnemies) return a.Team != referenceTeam;
                if (wantAllies) return a.Team == referenceTeam;
                return false;
            });
        }

        void HandleChosen(IActor actor)
        {
            Debug.Log($"[Sandbox] Target chosen: {actor?.Name}");
        }
    }
}