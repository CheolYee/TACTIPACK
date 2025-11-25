using UnityEngine;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Save
{
    public class SaveLoadBootstrap : MonoBehaviour
    {
        private void Start()
        {
            var ctrl = GameSaveController.Instance;
            if (ctrl != null)
            {
                ctrl.LoadAll();
            }
        }
    }
}