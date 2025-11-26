using System.Collections.Generic;
using System.Linq;
using _00.Work.Scripts.Managers;
using _00.Work.WorkSpace.CheolYee._04.Scripts.UI.HealthBar;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Managers
{
    public class HudManager : MonoSingleton<HudManager>
    {
        public static bool HasInstance => Instance != null;

        private readonly List<HealthBarUi> _huds = new();

        public void Register(HealthBarUi hud)
        {
            if (hud == null) return;
            if (_huds.Contains(hud) == false)
            {
                _huds.Add(hud);
            }
        }

        public void Unregister(HealthBarUi hud)
        {
            if (hud == null) return;
            _huds.Remove(hud);
        }
        
        public void ShowOnly(HealthBarUi target)
        {
            foreach (var hud in _huds)
            {
                hud.SetVisible(hud == target);
            }
        }
        
        public void ShowOnly(IEnumerable<HealthBarUi> targets)
        {
            var targetSet = new HashSet<HealthBarUi>(targets ?? Enumerable.Empty<HealthBarUi>());
            
            foreach (var hud in _huds)
            {
                hud.SetVisible(targetSet.Contains(hud));
            }
        }
        
        public void ShowAll() => _huds.ForEach(hud => hud.SetVisible(hud == true));
        public void HideAll() => _huds.ForEach(hud => hud.SetVisible(false));
        
        
    }
}