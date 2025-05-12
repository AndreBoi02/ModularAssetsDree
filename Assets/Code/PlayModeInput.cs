using UnityEngine;
using UnityEngine.InputSystem;

namespace ProceduralLevelDesign {
    public class PlayModeInput : MonoBehaviour {
        LevelBuilder _levelBuilder;
        Vector2 _mousePosition;


        private void Start() {
            _levelBuilder = GetComponent<LevelBuilder>();
        }

        void Update() {
            _mousePosition = Input.mousePosition;
        }

        public void CreateModule(InputAction.CallbackContext context) {
            if (context.canceled) {
                _levelBuilder.CreateLevel(_mousePosition, 1);
            }
        }

        public void DeleteModule(InputAction.CallbackContext context) {
            if (context.canceled) {
                _levelBuilder.DeleteLevel(_mousePosition, 1);
            }
        }

        public void ClearModules(InputAction.CallbackContext context) {
            if (context.canceled) {
                _levelBuilder.ClearLevel();
            }
        }
    }
}
