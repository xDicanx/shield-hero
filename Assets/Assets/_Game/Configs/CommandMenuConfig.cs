using System;
using System.Collections.Generic;
using UnityEngine;
using SH.Core;

namespace SH.Core
{
    [CreateAssetMenu(fileName = "CommandMenuConfig", menuName = "ShieldHero/Configs/Command Menu Config", order = 20)]
    public class CommandMenuConfig : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public bool enabled = true;
            public ActionType action = ActionType.Attack;
            public string label = "Attack";
            public Sprite icon;
            public KeyCode hotkey = KeyCode.None; // opcional para sandbox
        }

        [Header("Items del menú (orden de izquierda a derecha)")]
        public List<Entry> entries = new List<Entry>
        {
            new Entry{ enabled=true, action=ActionType.Attack,      label="Attack",      hotkey=KeyCode.A },
            new Entry{ enabled=true, action=ActionType.Defend,      label="Defend",      hotkey=KeyCode.D },
            new Entry{ enabled=true, action=ActionType.Wait,        label="Wait",        hotkey=KeyCode.W },
            new Entry{ enabled=true, action=ActionType.ShieldSkill, label="ShieldSkill", hotkey=KeyCode.S },
        };
    }
}