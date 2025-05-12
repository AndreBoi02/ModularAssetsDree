using UnityEditor;
using UnityEngine;

namespace ProceduralLevelDesign{
    [CustomEditor (typeof(LevelBuilder))]
    public class LevelBuilderEditor : Editor {
        LevelBuilder _levelBuilder;

        public override void OnInspectorGUI () {
            if (_levelBuilder == null) {
                _levelBuilder = (LevelBuilder)target;
            }
            DrawDefaultInspector();
            if (GUILayout.Button("CreateMatrix") ) {
                _levelBuilder.CreateMatrix();
            }
            if (GUILayout.Button("ClearMatrix")) {
                _levelBuilder.ClearMatrix();
            }
            if (GUILayout.Button("SetNeighbors")) {
                _levelBuilder.SetNeighbors();
            }
        }
    }
}
