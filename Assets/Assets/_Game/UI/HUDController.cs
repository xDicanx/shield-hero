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
            if (!loop) return;

            // Turno actual (si el TurnLoop expone CurrentActorName; si no, lo inferimos)
            string turnOwner = GetCurrentTurnOwnerName();
            turnText.text = $"Turn: {loop.CurrentActor?.Name ?? "-"}   (A=Attack  D=Defend  W=Wait  S=Shield)";

            // Listas
            var all = loop.AliveActors().ToList(); // vivos
            var allWithDead = GetAllActors(loop);  // vivos + muertos para mostrar estado real

            var players = allWithDead.Where(a => a.Team == Team.Player).ToList();
            var enemies = allWithDead.Where(a => a.Team == Team.Enemy).ToList();

            playersText.text = BuildSideBlock(players, "PLAYERS");
            enemiesText.text = BuildSideBlock(enemies, "ENEMIES");

            // Cargas (si hay)
            int c = 0; int max = 5;
            if (hero)
            {
                var sc = hero.GetComponent<ShieldChargeSystem>();
                if (sc) { c = sc.Charges; max = sc.MaxCharges; }
            }
            chargesText.text = $"Shield Charges: {c}/{max}";
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
            // Si agregas una propiedad pública en TurnLoop (recomendado):
            // public IActor CurrentActor { get; private set; }
            return loop.CurrentActor?.Name ?? "-";

            // Si aún no la tienes, devolvemos "-" y luego puedes añadirla (ver abajo).
            return "-";
        }

        // Recupera todos (vivos y muertos) leyendo el orden original (privado en TurnLoop),
        // o, si no es accesible, cae al truco de buscar componentes en escena (suficiente para el MVP).
        System.Collections.Generic.List<IActor> GetAllActors(TurnLoop tl)
        {
            // Truco: recoge PlayerActor y EnemyActor en escena; mantiene nombres y HP actuales.
            var list = FindObjectsOfType<MonoBehaviour>()
                       .OfType<IActor>()
                       .ToList();
            // Orden no garantizado; para MVP da igual.
            return list;
        }
    }
}
