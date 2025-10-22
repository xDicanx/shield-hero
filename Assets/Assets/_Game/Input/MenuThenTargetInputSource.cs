using System;
using System.Linq;
using UnityEngine;
using SH.Core;
using SH.UI;
using SH.Actors;

namespace SH.Input
{
    [AddComponentMenu("Input/MenuThenTargetInputSource")]
    public class MenuThenTargetInputSource : MonoBehaviour, IPlayerInputSource
    {
        [Header("References")]
        [SerializeField] CommandMenuInputSource menuSource;
        [SerializeField] CommandMenuUI menuUI;            // NUEVO: para abrir/cerrar visual
        [SerializeField] TargetSelectorUI targetSelector;
        [SerializeField] TurnLoop loop;

        [Header("Targeting")]
        [SerializeField] TargetFilter defaultFilter = TargetFilter.EnemiesOnly;

        [Header("Debug")]
        [SerializeField] bool debugLogs = false;

        Action<ActionData> pendingCallback;
        IActor pendingActor;
        ActionType pendingActionType;

        void Awake()
        {
            if (!menuSource)     menuSource     = FindObjectOfType<CommandMenuInputSource>(includeInactive: true);
            if (!menuUI)         menuUI         = FindObjectOfType<CommandMenuUI>(includeInactive: true);
            if (!targetSelector) targetSelector = FindObjectOfType<TargetSelectorUI>(includeInactive: true);
            if (!loop)           loop           = FindObjectOfType<TurnLoop>(includeInactive: true);

            if (debugLogs)
                Debug.Log($"[MenuThenTarget] Awake → menuSource={(menuSource?menuSource.name:"null")}, targetSelector={(targetSelector?targetSelector.name:"null")}, loop={(loop?loop.name:"null")}");
        }

        [ContextMenu("Auto-Wire References")]
        void AutoWire()
        {
            Awake();
            if (debugLogs) Debug.Log("[MenuThenTarget] Auto-Wire executed.");
        }

        void Reset() => Awake();

        public void RequestAction(IActor actor, Action<ActionData> onDecision)
        {
            pendingActor = actor;
            pendingCallback = onDecision;

            if (!menuSource)
            {
                if (debugLogs) Debug.LogWarning("[MenuThenTarget] No CommandMenuInputSource en escena.");
                CancelRequest(); return;
            }

            // Asegurar que el menú esté visible al iniciar la elección de acción.
            if (menuUI) menuUI.Open();

            // 1) Acción desde menú
            menuSource.RequestAction(actor, selected =>
            {
                pendingActionType = selected.Type;

                // Sin target → cerrar menú y finalizar
                if (pendingActionType == ActionType.Defend || pendingActionType == ActionType.Wait)
                {
                    if (menuUI) menuUI.Close();
                    Submit(new ActionData(pendingActionType, pendingActor, null, 0));
                    return;
                }

                // 2) Requiere target → cerrar menú y abrir selector
                var candidates = CollectCandidates(defaultFilter).ToArray();
                if (candidates.Length == 0)
                {
                    if (debugLogs) Debug.LogWarning("[MenuThenTarget] Sin candidatos. Degradar a Wait.");
                    if (menuUI) menuUI.Close();
                    Submit(new ActionData(ActionType.Wait, pendingActor, null, 0));
                    return;
                }

                if (menuUI) menuUI.Close();

                if (!targetSelector)
                {
                    if (debugLogs) Debug.LogWarning("[MenuThenTarget] No TargetSelectorUI. Usar primer candidato.");
                    Submit(BuildActionWithTarget(candidates[0], pendingActionType));
                    return;
                }

                if (debugLogs) Debug.Log($"[MenuThenTarget] Abriendo selector con {candidates.Length} candidatos.");
                targetSelector.OnTargetChosen += OnTargetChosenOnce;
                targetSelector.SetFilter(defaultFilter);
                targetSelector.Open(candidates);
            });
        }

        public void CancelRequest()
        {
            pendingCallback = null;
            pendingActor = null;
            pendingActionType = ActionType.Wait;

            if (targetSelector) targetSelector.Close();
            if (menuSource)     menuSource.CancelRequest();
            if (menuUI)         menuUI.Close();
        }

        void OnTargetChosenOnce(IActor target)
        {
            if (targetSelector)
                targetSelector.OnTargetChosen -= OnTargetChosenOnce;

            Submit(BuildActionWithTarget(target, pendingActionType));
        }

        void Submit(ActionData data)
        {
            if (debugLogs) Debug.Log($"[MenuThenTarget] Submit {data}");
            var cb = pendingCallback;
            CancelRequest();
            cb?.Invoke(data);
        }

        ActionData BuildActionWithTarget(IActor target, ActionType type)
        {
            int amount = 0;

            if (type == ActionType.Attack && pendingActor is ActorBase ab)
                amount = ab.Attack;

            if (type == ActionType.ShieldSkill)
            {
                var mb = pendingActor as MonoBehaviour;
                var shield = mb ? mb.GetComponent<ShieldChargeSystem>() : null;
                int bonus = shield ? shield.ConsumeForBonus() : 0;
                amount = Mathf.Max(1, bonus);
            }

            return new ActionData(type, pendingActor, target, amount);
        }

        System.Collections.Generic.IEnumerable<IActor> CollectCandidates(TargetFilter filter)
        {
            var alive = loop != null ? loop.AliveActors() : System.Linq.Enumerable.Empty<IActor>();
            if (pendingActor == null) return alive;

            return filter switch
            {
                TargetFilter.EnemiesOnly => alive.Where(a => a.Team != pendingActor.Team),
                TargetFilter.AlliesOnly  => alive.Where(a => a.Team == pendingActor.Team),
                _                        => alive,
            };
        }
    }
}