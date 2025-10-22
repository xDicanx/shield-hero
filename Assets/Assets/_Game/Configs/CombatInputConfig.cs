using UnityEngine;

namespace SH.Core
{
    public enum InputMode { Legacy, UI }

    // Flujo UI: solo menú (acción) o menú + selección de objetivo.
    public enum UIFlow { MenuOnly, MenuThenTarget }

    [CreateAssetMenu(fileName = "CombatInputConfig", menuName = "ShieldHero/Configs/Combat Input Config", order = 10)]
    public class CombatInputConfig : ScriptableObject
    {
        [Header("Global Input Mode")]
        public InputMode Mode = InputMode.Legacy;

        [Header("UI Flow")]
        public UIFlow Flow = UIFlow.MenuOnly;
    }
}