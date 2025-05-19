using UnityEditor;
using UnityEngine;

namespace ProceduralLevelDesign {
    [CustomEditor(typeof(LevelBuilder))]
    public class LevelBuilderEditor : Editor {
        #region Reference
        LevelBuilder _levelBuilder;
        #endregion
        #region UnityMethods
        public override void OnInspectorGUI() {
            if (_levelBuilder == null) {
                _levelBuilder = (LevelBuilder)target;
            }
            DrawDefaultInspector();
            if (GUILayout.Button("SpawnThemHoes")) {
                _levelBuilder.CreateLevel();
            }
            if (GUILayout.Button("CutThemHoes")) {
                Maze maze = new Maze() {
                    minX = 0,
                    minY = 0,
                    maxX = _levelBuilder._XMatrixAxis - 1,
                    maxY = _levelBuilder._YMatrixAxis - 1,
                };
                _levelBuilder.BinarySpacePartition(maze);
            }
            if (GUILayout.Button("ClearThemHoes")) {
                _levelBuilder.ClearLevel();
            }
        }
        #endregion
    }
}
