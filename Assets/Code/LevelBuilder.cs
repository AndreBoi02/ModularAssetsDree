using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ProceduralLevelDesign {
    #region Interfaces
    public interface ILevelEditor {
        public void ClearLevel();
        public void DeleteModule(Vector2 val, int i);
        public void CreateModule(Vector2 val, int i);
    }
    #endregion
    #region Struc

    [System.Serializable]
    public struct Maze {
        [SerializeField] public int minX;
        [SerializeField] public int maxX;
        [SerializeField] public int minY;
        [SerializeField] public int maxY;

        public int width => maxX - minX;
        public int heigth => maxY - minY;
        //[SerializeField] public int width() { return maxX - minX; }
        //[SerializeField] public int heigth() { return maxY - minY; }

        [SerializeField] public bool isSilceableX;
        [SerializeField] public bool isSilceableY;
    }

    #endregion

    public class LevelBuilder : MonoBehaviour, ILevelEditor {
        #region References
        [SerializeField] Camera _cam;
        [SerializeField] GameObject _modulePrefab;
        [SerializeField]List<GameObject> _corners;
        #endregion
        #region InternalData
        List<Module> _allModulesInScene = new List<Module>();
        Module[,] _matrix;
        public int _YMatrixAxis;
        public int _XMatrixAxis;
        #endregion
        #region RuntimeVar
        GameObject _moduleInstance;

        Ray _rayFromSceneCamera;
        RaycastHit _rayCastHit;
        Vector3 _modulePosition;
        #endregion

        public void CreateMaze() {
            CreateMatrix();
            GameObject tempPrefab;
            for (int i = 0; i < _XMatrixAxis; i++) {
                for (int j = 0; j < _YMatrixAxis; j++) {
                    tempPrefab = Instantiate(_modulePrefab);
                    tempPrefab.transform.parent = transform;
                    tempPrefab.transform.position = new Vector3(i, tempPrefab.transform.position.y, j);

                    _allModulesInScene.Add(tempPrefab.GetComponent<Module>());
                    _matrix[(int)tempPrefab.transform.position.x, (int)tempPrefab.transform.position.z] = tempPrefab.GetComponent<Module>();
                    SetNeighbors();
                }
            }
        }

        public void ClearLevel() {
            foreach (Module module in _allModulesInScene) {
                module.gameObject.SetActive(true);
                DestroyImmediate(module.gameObject);

            }
            _allModulesInScene.Clear();
            ClearMatrix();
        }

        public void DeleteModule(Vector2 val, int i) {
            _rayFromSceneCamera = i == 0 ? HandleUtility.GUIPointToWorldRay(val) : _cam.ScreenPointToRay(val);
            if (RayHitLayout())
                return;
            _moduleInstance = _rayCastHit.collider.transform.parent.gameObject;
            _allModulesInScene.Remove(_moduleInstance.GetComponent<Module>());
            DestroyImmediate(_moduleInstance);
            SetNeighbors();
        }

        public void CreateModule(Vector2 val, int i) {
            CreateMatrix();
            _rayFromSceneCamera = i == 0? HandleUtility.GUIPointToWorldRay(val) : _cam.ScreenPointToRay(val);
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

        public void CreateMatrix() {
            if (_matrix != null)
                return;

            _matrix = new Module[_XMatrixAxis, _YMatrixAxis];

            _corners[0].transform.position = new Vector3(0, 0, 0);
            _corners[1].transform.position = new Vector3(_XMatrixAxis, 0, 0);
            _corners[2].transform.position = new Vector3(0, 0, _YMatrixAxis);
            _corners[3].transform.position = new Vector3(_XMatrixAxis, 0, _YMatrixAxis);
        }

        public void ClearMatrix() {
            _matrix =null;
            foreach (GameObject gameObject in _corners) {
                gameObject.transform.position = new Vector3(0, 0, 0);
            }
        }

        bool RayHitLayout() {
            Debug.DrawRay(_rayFromSceneCamera.origin, _rayFromSceneCamera.direction * 10000f, Color.green, 5f);
            return Physics.Raycast(_rayFromSceneCamera, out _rayCastHit, 10000f) && _rayCastHit.collider.gameObject.layer == LayerMask.NameToLayer("Layout");
        }

        public void SetNeighbors() {
            foreach (Module module in _allModulesInScene) {
                module.GetNeighbors(GiveNeighbors(module));
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
            return validX && validY;
        }

        public List<Module> GiveNeighbors(Module module) {
            List<Module> neighbors = new List<Module>();

            int x = (int)module.transform.position.x;
            int z = (int)module.transform.position.z;

            module.SetWestNeigh(x > 0 && _matrix[x - 1, z] && _matrix[x - 1, z].gameObject.activeSelf);
            //module.SetWestNeigh(x > 0 && _matrix[x - 1, z]);
            if (module.westNeighbor)
                neighbors.Add(_matrix[x - 1, z]);

            module.SetEastNeigh(x < _XMatrixAxis - 1 && _matrix[x + 1, z]);
            //module.SetEastNeigh(x < _XMatrixAxis - 1 && _matrix[x + 1, z] && _matrix[x + 1, z].gameObject.activeSelf);
            if (module.eastNeighbor)
                neighbors.Add(_matrix[x + 1, z]);

            module.SetSouthNeigh(z > 0 && _matrix[x, z - 1] && _matrix[x, z - 1].gameObject.activeSelf);
            //module.SetSouthNeigh(z > 0 && _matrix[x, z - 1]);
            if (module.southNeighbor)
                neighbors.Add(_matrix[x, z - 1]);
            
            module.SetNorthNeigh(z < _YMatrixAxis - 1 && _matrix[x, z + 1] && _matrix[x, z + 1].gameObject.activeSelf);
            //module.SetNorthNeigh(z < _YMatrixAxis - 1 && _matrix[x, z + 1]);
            if (module.northNeighbor)
                neighbors.Add(_matrix[x, z + 1]);

            module.SetSouthWestNeigh(x > 0 && z > 0 && _matrix[x - 1, z - 1] && _matrix[x - 1, z - 1].gameObject.activeSelf);
            //module.SetSouthWestNeigh(x > 0 && z > 0 && _matrix[x - 1, z - 1]);
            if (module.southWestNeighbor)
                neighbors.Add(_matrix[x - 1, z - 1]);

            module.SetSouthEastNeigh(x < _XMatrixAxis - 1 && z > 0 && _matrix[x + 1, z - 1] && _matrix[x + 1, z - 1].gameObject.activeSelf);
            //module.SetSouthEastNeigh(x < _XMatrixAxis - 1 && z > 0 && _matrix[x + 1, z - 1]);
            if (module.southEastNeighbor)
                neighbors.Add(_matrix[x + 1, z - 1]);

            module.SetNorthEastNeigh(x < _XMatrixAxis - 1 && z < _YMatrixAxis - 1 && _matrix[x + 1, z + 1] && _matrix[x + 1, z + 1].gameObject.activeSelf);
            //module.SetNorthEastNeigh(x < _XMatrixAxis - 1 && z < _YMatrixAxis - 1 && _matrix[x + 1, z + 1]);
            if(module.northEastNeighbor)
            neighbors.Add(_matrix[x + 1, z + 1]);

            module.SetNorthWestNeigh(x > 0 && z < _YMatrixAxis - 1 && _matrix[x - 1, z + 1] && _matrix[x - 1, z + 1].gameObject.activeSelf);
            //module.SetNorthWestNeigh(x > 0 && z < _YMatrixAxis - 1 && _matrix[x - 1, z + 1]);
            if(module.northWestNeighbor)
                neighbors.Add(_matrix[x - 1, z + 1]);

            return neighbors;
        }

        [SerializeField] int minMazeSizeX;
        [SerializeField] int minMazeSizeY;

        public void BinarySpacePartition(Maze maze) {
            maze.isSilceableX = maze.width > minMazeSizeX * 2;
            maze.isSilceableY = maze.heigth > minMazeSizeY * 2;

            if (!maze.isSilceableX || !maze.isSilceableY)
                return;

            if (maze.isSilceableX && maze.isSilceableY) {
                int randomCutAxis = Random.Range(0, 2);
                if (randomCutAxis == 0) {
                    maze.isSilceableX = false;
                }
                else {
                    maze.isSilceableY = false;
                }
            }
            if (maze.isSilceableX && !maze.isSilceableY) {
                CutMazefromX(maze);
            }

            if (!maze.isSilceableX && maze.isSilceableY) {
                CutMazefromY(maze);
            }

        } 

        void CutMazefromX(Maze maze) {
            int randomCut = Random.Range(maze.minX + minMazeSizeX + 1, maze.maxX - minMazeSizeX - 1);
            for (int i = maze.minY; i <= maze.maxY; i++) {
                if (_matrix[randomCut, i]?.transform.position.z >= maze.minY && _matrix[randomCut, i]?.transform.position.z <= maze.maxY) {
                    _matrix[i, randomCut]?.gameObject.SetActive(false);
                    SetNeighbors();
                }
            }
            Maze maze1 = new Maze() {
                minX = maze.minX,
                maxX = randomCut - 1,
                minY = maze.minY,
                maxY = maze.maxY
            };

            Maze maze2 = new Maze() {
                minX = randomCut + 1,
                maxX = maze.maxX,
                minY = maze.minY,
                maxY = maze.maxY
            };
            BinarySpacePartition(maze1);
            BinarySpacePartition(maze2);
        }

        void CutMazefromY(Maze maze) {
            int randomCut = Random.Range(maze.minY + minMazeSizeY + 1, maze.maxY - minMazeSizeY - 1);
            for (int i = maze.minX; i <= maze.maxX; i++) {
                if (_matrix[i, randomCut]?.transform.position.x >= maze.minX && _matrix[i, randomCut]?.transform.position.x <= maze.maxX) {
                    _matrix[i, randomCut]?.gameObject.SetActive(false);
                    SetNeighbors();
                }
            }
            Maze maze1 = new Maze() {
                minX = maze.minX,
                maxX = maze.maxX,
                minY = maze.minY,
                maxY = randomCut - 1
            };

            Maze maze2 = new Maze() {
                minX = maze.minX,
                maxX = maze.maxX,
                minY = randomCut + 1,
                maxY = maze.maxY
            };
            BinarySpacePartition(maze1);
            BinarySpacePartition(maze2);
        }
    }
}
