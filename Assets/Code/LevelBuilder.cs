using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using NUnit.Framework;
using Unity.VisualScripting;

namespace ProceduralLevelDesign {
    #region Interfaces
    public interface ILevelEditor {
        public void ClearLevel();
        public void DeleteLevel(Vector2 val, int i);
        public void CreateLevel(Vector2 val, int i);
    }
    #endregion

    public class LevelBuilder : MonoBehaviour, ILevelEditor {
        #region Parameters
        [SerializeField] Camera _cam;
        [SerializeField] GameObject _modulePrefab;
        GameObject _moduleInstance;

        [SerializeField] int _XMatrixAxis;
        [SerializeField] int _YMatrixAxis;
        Module[,] _matrix;
        #endregion

        #region InternalData
        [SerializeField] List<Module> _allModulesInScene;
        #endregion

        [SerializeField]List<GameObject> _corners;

        #region RuntimeVar
        Ray _rayFromSceneCamera;
        RaycastHit _rayCastHit;
        Vector3 _modulePosition;
        #endregion

        public void ClearLevel() {
            foreach (Module module in transform.GetComponentsInChildren<Module>()) {
                DestroyImmediate(module.gameObject);
            }
            _allModulesInScene.Clear();
            _matrix = null;
        }

        public void DeleteLevel(Vector2 val, int i) {
            if (i == 0) {
                GetRayFromSceneCamera(val);
            } else {
                GetRayFromPlayCamera(val);
            }
            if (RayHitLayout())
                return;
            _moduleInstance = _rayCastHit.collider.transform.parent.gameObject;
            _allModulesInScene.Remove(_moduleInstance.GetComponent<Module>());
            DestroyImmediate(_moduleInstance);
            SetNeighbors();
        }

        public void CreateLevel(Vector2 val, int i) {
            if (i == 0) {
                GetRayFromSceneCamera(val);
            } 
            else {
                GetRayFromPlayCamera(val);
            }
            if (!RayHitLayout())
                return;

            if (CheckIfValidCoordinate(_rayCastHit.point) && _matrix != null) {
                _moduleInstance = Instantiate(_modulePrefab);
                _moduleInstance.transform.parent = transform;
                _modulePosition = _rayCastHit.point;
                _modulePosition.x = (int)_modulePosition.x;
                _modulePosition.y = (int)_modulePosition.y;
                _modulePosition.z = (int)_modulePosition.z;
                _moduleInstance.transform.position = _modulePosition;

                _allModulesInScene.Add(_moduleInstance.GetComponent<Module>());
                _matrix[(int)_modulePosition.x, (int)_modulePosition.z] = _moduleInstance.GetComponent<Module>();
                SetNeighbors();
            }
            
        }

        public void GetRayFromSceneCamera(Vector2 val) {
            _rayFromSceneCamera = HandleUtility.GUIPointToWorldRay(val);
        }

        public void GetRayFromPlayCamera(Vector2 val) {
            _rayFromSceneCamera = _cam.ScreenPointToRay(val);
        }

        bool RayHitLayout() {
            Debug.DrawRay(_rayFromSceneCamera.origin, _rayFromSceneCamera.direction * 10000f, Color.green, 5f);
            if (Physics.Raycast(_rayFromSceneCamera, out _rayCastHit, 10000f)) {
                if (_rayCastHit.collider.gameObject.layer == LayerMask.NameToLayer("Layout")) {
                    return true;
                }
            }
            return false;
        }

        public void CreateMatrix() {
            _matrix = new Module[_XMatrixAxis, _YMatrixAxis];

            _corners[0].transform.position = new Vector3(0, 0, 0);
            _corners[1].transform.position = new Vector3(_XMatrixAxis, 0, 0);
            _corners[2].transform.position = new Vector3(0, 0, _YMatrixAxis);
            _corners[3].transform.position = new Vector3(_XMatrixAxis, 0, _YMatrixAxis);
        }

        public void ClearMatrix() {
            _allModulesInScene.Clear();
            _matrix =null;
            foreach (GameObject gameObject in _corners) {
                gameObject.transform.position = new Vector3(0, 0, 0);
            }
        }

        public void SetNeighbors() {
            foreach (Module module in _allModulesInScene) {
                module.neighbors = GetNeighbors(module);
                module.TurnOffWalls();
                module.TurnOffPillars();
            }
        }

        bool CheckIfValidCoordinate(Vector3 val) {
            bool validX = false; 
            bool validY = false;
            for (int i = 0; i < _XMatrixAxis - 1; i++) {
                for (int j = 0; j < _YMatrixAxis - 1; j++) {
                    
                    if ((int)val.x == i) {
                        validX = true;
                    }
                    if ((int)val.z == j) {
                        validY = true;
                    }
                }
            }
            if(validX && validY) {
                return true;
            } 
            else {
                return false;
            }
        }

        public List<Module> GetNeighbors(Module module) {
            List<Module> neighbors = new List<Module>();

            int x = (int)module.transform.position.x;
            int z = (int)module.transform.position.z;

            if (x > 0 && _matrix[x - 1, z]) {
                neighbors.Add(_matrix[x - 1, z]);
                module.westNeighbor = true;
            } else {
                module.westNeighbor = false;
            }
            if (x < _XMatrixAxis - 1 && _matrix[x + 1, z]) {
                neighbors.Add(_matrix[x + 1, z]);
                module.eastNeighbor = true;
            } else {
                module.eastNeighbor = false;
            }
            if (z > 0 && _matrix[x, z - 1]) {
                neighbors.Add(_matrix[x, z - 1]);
                module.southNeighbor = true;
            } else {
                module.southNeighbor = false;
            }
            if (z < _YMatrixAxis - 1 && _matrix[x, z + 1]) {
                neighbors.Add(_matrix[x, z + 1]);
                module.northNeighbor = true;
            } else {
                module.northNeighbor = false;
            }

            if (x > 0 && z > 0 && _matrix[x - 1, z - 1]) {
                neighbors.Add(_matrix[x - 1, z - 1]);
                module.southWestNeighbor = true;
            } else {
                module.southWestNeighbor = false;
            }
            if (x < _XMatrixAxis - 1 && z > 0 && _matrix[x + 1, z - 1]) {
                neighbors.Add(_matrix[x - 1, z + 1]);
                module.southEastNeighbor = true;
            } else {
                module.southEastNeighbor = false;
            }
            if (x < _XMatrixAxis - 1 && z < _YMatrixAxis - 1  && _matrix[x + 1, z + 1]) {
                neighbors.Add(_matrix[x + 1, z + 1]);
                module.northEastNeighbor = true;
            } else {
                module.northEastNeighbor = false;
            }
            if (x > 0 && z < _YMatrixAxis - 1 && _matrix[x - 1, z + 1]) {
                neighbors.Add(_matrix[x - 1, z + 1]);
                module.northWestNeighbor = true;
            } else {
                module.northWestNeighbor = false;
            }


            return neighbors;
        }
    }
}

