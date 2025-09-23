using UnityEngine;
using UnityEngine.InputSystem;

namespace _00.Work.WorkSpace.CheolYee._04.Scripts.Core
{
    [CreateAssetMenu(fileName = "playerInputSO", menuName = "SO/playerInput")]
    public class PlayerInputSo : ScriptableObject, Controls.IPlayerActions
    {
        private Controls _controls;

        public Vector2 MoveInput {get; private set;}
        public bool IsSpace {get; private set;}

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }
            
            _controls.Player.Enable();
        }

        private void OnDisable()
        {
            _controls.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnSpace(InputAction.CallbackContext context)
        {
            IsSpace = context.ReadValueAsButton();
        }
    }
}