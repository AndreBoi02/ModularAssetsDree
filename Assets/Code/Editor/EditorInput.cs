using UnityEngine;
using UnityEditor;

namespace ProceduralLevelDesign {
    public class EditorInput {
        #region LocalMethods

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void ScriptsHasBeenReloaded() {
            SceneView.duringSceneGui += DuringSceneGui;
        }

        #endregion
        #region DelegateMethods

        private static void DuringSceneGui(SceneView sceneView) {
            Event e = Event.current;

            LevelBuilder levelBuilder = GameObject.FindFirstObjectByType<LevelBuilder>();
            if ( e.type == EventType.KeyUp && e.keyCode == KeyCode.Delete) {
                levelBuilder?.ClearLevel();
            }
            if ( e.type == EventType.KeyUp && e.keyCode == KeyCode.Backspace) {
                levelBuilder?.DeleteModule(e.mousePosition , 0);
            }
            if( e.type == EventType.MouseUp && e.button == 0 ) {
                levelBuilder?.CreateModule(e.mousePosition, 0);
            }
        }

        #endregion
    }
}
