using UnityEngine;

namespace SH.Core
{
    public enum TargetFilter
    {
        EnemiesOnly,
        AlliesOnly,
        AnyAlive
    }

    [CreateAssetMenu(fileName = "TargetSelectorConfig", menuName = "ShieldHero/Configs/Target Selector Config", order = 30)]
    public class TargetSelectorConfig : ScriptableObject
    {
        [Header("Lógica")]
        public TargetFilter filter = TargetFilter.EnemiesOnly;
        public bool wrapNavigation = true;         // circular al navegar
        public float worldOffsetY = 1.5f;          // offset vertical sobre el actor

        [Header("Controles")]
        public KeyCode confirmKey = KeyCode.Return;
        public KeyCode altConfirmKey = KeyCode.Space;
        public KeyCode cancelKey = KeyCode.Escape;
        public KeyCode leftKey = KeyCode.A;
        public KeyCode rightKey = KeyCode.D;
        public KeyCode upKey = KeyCode.W;
        public KeyCode downKey = KeyCode.S;

        [Header("Visual")]
        public Color normalColor = new Color(1f, 1f, 1f, 0.85f);
        public Color selectedColor = new Color(1f, 0.9f, 0.4f, 1f);
        public float selectedScale = 1.2f;
    }
}