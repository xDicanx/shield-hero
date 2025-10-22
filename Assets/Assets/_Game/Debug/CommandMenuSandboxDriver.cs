using UnityEngine;
using SH.Core;
using SH.UI;

namespace SH.Debugging
{
    // Driver para probar CommandMenuUI en una escena aislada.
    public class CommandMenuSandboxDriver : MonoBehaviour
    {
        public CommandMenuUI menu;

        void Reset()
        {
            if (!menu) menu = FindObjectOfType<CommandMenuUI>();
        }

        void OnEnable()
        {
            if (menu) menu.OnCommandSelected += HandleSelected;
        }

        void OnDisable()
        {
            if (menu) menu.OnCommandSelected -= HandleSelected;
        }

        void HandleSelected(ActionType type)
        {
            Debug.Log($"[Sandbox] Selected: {type}");
        }
    }
}