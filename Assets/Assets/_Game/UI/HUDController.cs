using UnityEngine;
using System.Linq;
using System.Text;
using TMPro;
using SH.Core;
using SH.Actors;

namespace SH.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Referencias")]
        public TurnLoop loop;                  // arrástralo desde la escena
        public TextMeshProUGUI turnText;       // "Turn: Hero (A/D/W/S)"
        public TextMeshProUGUI playersText;    // HP de héroe/aliado(s)
        public TextMeshProUGUI enemiesText;    // HP de enemigos
        public TextMeshProUGUI chargesText;    // Cargas del héroe

        [Header("Opcional")]
        public PlayerActor hero;               // para leer ShieldChargeSystem

        void Reset()
        {
            loop = FindObjectOfType<TurnLoop>();
            if (!hero) hero = FindObjectsOfType<PlayerActor>().FirstOrDefault();
        }

        void Update()
        {
            var hudData = FetchHUDData();
            var formatted = FormatHUDText(hudData);
            UpdateHUDFields(formatted);
        }

        // Encapsula datos necesarios para HUD en un struct
        private struct HUDData
        {
            public string turnOwner;
            public System.Collections.Generic.List<IActor> players;
            public System.Collections.Generic.List<IActor> enemies;
            public int charges, maxCharges;
        }

        /// <summary>
        /// Obtiene los datos base que usará el HUD.
        /// </summary>
        private HUDData FetchHUDData()
        {
            var data = new HUDData();
            data.turnOwner = GetCurrentTurnOwnerName();
            var allWithDead = GetAllActors(loop);
            data.players = allWithDead.Where(a => a.Team == Team.Player).ToList();
            data.enemies = allWithDead.Where(a => a.Team == Team.Enemy).ToList();

            int c = 0, max = 5;
            if (hero)
            {
                var sc = hero.GetComponent<ShieldChargeSystem>();
                if (sc) { c = sc.Charges; max = sc.MaxCharges; }
            }
            data.charges = c;
            data.maxCharges = max;
            return data;
        }

        /// <summary>
        /// Convierte los datos base en strings para el HUD.
        /// </summary>
        private (string turn, string players, string enemies, string charges) FormatHUDText(HUDData data)
        {
            string turnStr = $"Turn: {data.turnOwner}   (A=Attack  D=Defend  W=Wait  S=Shield)";
            string playersStr = BuildSideBlock(data.players, "PLAYERS");
            string enemiesStr = BuildSideBlock(data.enemies, "ENEMIES");
            string chargesStr = $"Shield Charges: {data.charges}/{data.maxCharges}";
            return (turnStr, playersStr, enemiesStr, chargesStr);
        }

        /// <summary>
        /// Asigna los strings formateados a los campos visuales.
        /// </summary>
        private void UpdateHUDFields((string turn, string players, string enemies, string charges) formatted)
        {
            turnText.text = formatted.turn;
            playersText.text = formatted.players;
            enemiesText.text = formatted.enemies;
            chargesText.text = formatted.charges;
        }

        string BuildSideBlock(System.Collections.Generic.IEnumerable<IActor> actors, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);
            foreach (var a in actors)
            {
                string life = a.IsAlive ? $"{a.HP}/{a.MaxHP}" : "DEAD";
                sb.AppendLine($"{a.Name,-12}  {life}");
            }
            return sb.ToString();
        }

        string GetCurrentTurnOwnerName()
        {
            return loop.CurrentActor?.Name ?? "-";
        }

        System.Collections.Generic.List<IActor> GetAllActors(TurnLoop tl)
        {
            var list = FindObjectsOfType<MonoBehaviour>()
                       .OfType<IActor>()
                       .ToList();
            return list;
        }
    }
}