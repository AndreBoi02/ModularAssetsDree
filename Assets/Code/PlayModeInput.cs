using UnityEngine;
using UnityEngine.InputSystem;

namespace ProceduralLevelDesign {
    public class PlayModeInput : MonoBehaviour {
        #region Reference
        LevelBuilder _levelBuilder => FindAnyObjectByType<LevelBuilder>();
        #endregion
        #region PublicMethods
        public void CreateModule(InputAction.CallbackContext context) {
            if (context.canceled) {
                _levelBuilder.CreateModule(Input.mousePosition, 1);
            }
        }

        public void DeleteModule(InputAction.CallbackContext context) {
            if (context.canceled) {
                _levelBuilder.DeleteModule(Input.mousePosition, 1);
            }
        }

        public void ClearModules(InputAction.CallbackContext context) {
            if (context.canceled) {
                _levelBuilder.ClearLevel();
            }
        }
        #endregion
    }
}
