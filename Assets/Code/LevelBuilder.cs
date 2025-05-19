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
        public enum LastCutType {
            none,
            horizontal,
            vertical
        }

        [SerializeField] public LastCutType lastCutType;
        [SerializeField] public int lastRandomBridge;
        [SerializeField] public int minX;
        [SerializeField] public int maxX;
        [SerializeField] public int minY;
        [SerializeField] public int maxY;
        [SerializeField] public int width => maxX - minX;
        [SerializeField] public int heigth => maxY - minY;
                         
        [SerializeField] public bool isSilceableX;
        [SerializeField] public bool isSilceableY;
    }
    #endregion

    //si el corte es en diferente eje y el randomCut y el randomBridge son lo mismo entonces tenemos que escojer un nuevo randomCut

    public class LevelBuilder : MonoBehaviour, ILevelEditor {
        #region Parameters
        [SerializeField] Camera _cam;
        [SerializeField] GameObject _modulePrefab;
        [SerializeField]List<GameObject> _corners;
        #endregion
        #region InternalData
        List<Module> _allModulesInScene = new List<Module>();
        Module[,] _matrix;
        #endregion
        #region RuntimeVar
        Ray _rayFromSceneCamera;
        RaycastHit _rayCastHit;
        [SerializeField] public int _XMatrixAxis;
        [SerializeField] public int _YMatrixAxis;
        [SerializeField] int minMazeSizeX;
        [SerializeField] int minMazeSizeY;
        #endregion
        #region PublicMethods
        public void CreateLevel() {
            CreateMatrix();
            for (int i = 0; i < _XMatrixAxis; i++) {
                for (int j = 0; j < _YMatrixAxis; j++) {
                    GameObject tempObj = Instantiate(_modulePrefab);
                    tempObj.transform.parent = transform;
                    tempObj.transform.position = new Vector3(i, _modulePrefab.transform.position.y, j);
                    _allModulesInScene.Add(tempObj.GetComponent<Module>());
                    _matrix[i, j] = tempObj.GetComponent<Module>();
                    SetNeighbors();
                }
            }
        }
        public void ClearLevel() {
            foreach (Module module in _allModulesInScene) {
                DestroyImmediate(module.gameObject);
            }
            _allModulesInScene.Clear();
            ClearMatrix();
        }
        public void CreateModule(Vector2 val, int i) {
            CreateMatrix();
            _rayFromSceneCamera = i == 0? HandleUtility.GUIPointToWorldRay(val) : _cam.ScreenPointToRay(val);
            if (!RayHitLayout())
                return;

            if (CheckIfValidCoordinate(_rayCastHit.point) && _matrix != null) {
                GameObject tempModule = Instantiate(_modulePrefab);
                tempModule.transform.parent = transform;
                tempModule.transform.position = new Vector3 ((int)_rayCastHit.point.x, (int)_rayCastHit.point.y, (int)_rayCastHit.point.z);

                _allModulesInScene.Add(tempModule.GetComponent<Module>());
                _matrix[(int)tempModule.transform.position.x, (int)tempModule.transform.position.z] = tempModule.GetComponent<Module>();
                SetNeighbors();
            }
        }
        public void DeleteModule(Vector2 val, int i) {
            _rayFromSceneCamera = i == 0 ? HandleUtility.GUIPointToWorldRay(val) : _cam.ScreenPointToRay(val);
            if (RayHitLayout())
                return;
            GameObject tempModule = _rayCastHit.collider.transform.parent.gameObject;
            _allModulesInScene.Remove(tempModule.GetComponent<Module>());
            DestroyImmediate(tempModule);
            SetNeighbors();
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
        public void SetNeighbors() {
            foreach (Module module in _allModulesInScene) {
                module.GetNeighbors(GiveNeighbors(module));
                module.TurnOffWalls();
                module.TurnOffPillars();
            }
        }
        public List<Module> GiveNeighbors(Module module) {
            List<Module> neighbors = new List<Module>();

            int x = (int)module.transform.position.x;
            int z = (int)module.transform.position.z;

            module.SetWestNeigh(x > 0 && _matrix[x - 1, z]);
            if (module.westNeighbor)
                neighbors.Add(_matrix[x - 1, z]);

            module.SetEastNeigh(x < _XMatrixAxis - 1 && _matrix[x + 1, z]);
            if (module.eastNeighbor)
                neighbors.Add(_matrix[x + 1, z]);

            module.SetSouthNeigh(z > 0 && _matrix[x, z - 1]);
            if (module.southNeighbor)
                neighbors.Add(_matrix[x, z - 1]);
            
            module.SetNorthNeigh(z < _YMatrixAxis - 1 && _matrix[x, z + 1]);
            if (module.northNeighbor)
                neighbors.Add(_matrix[x, z + 1]);

            module.SetSouthWestNeigh(x > 0 && z > 0 && _matrix[x - 1, z - 1]);
            if (module.southWestNeighbor)
                neighbors.Add(_matrix[x - 1, z - 1]);

            module.SetSouthEastNeigh(x < _XMatrixAxis - 1 && z > 0 && _matrix[x + 1, z - 1]);
            if (module.southEastNeighbor)
                neighbors.Add(_matrix[x + 1, z - 1]);

            module.SetNorthEastNeigh(x < _XMatrixAxis - 1 && z < _YMatrixAxis - 1 && _matrix[x + 1, z + 1]);
            if(module.northEastNeighbor)
            neighbors.Add(_matrix[x + 1, z + 1]);

            module.SetNorthWestNeigh(x > 0 && z < _YMatrixAxis - 1 && _matrix[x - 1, z + 1]);
            if(module.northWestNeighbor)
                neighbors.Add(_matrix[x - 1, z + 1]);

            return neighbors;
        }
        public void BinarySpacePartition(Maze maze) {
            maze.isSilceableX = maze.width > minMazeSizeX * 2 ? true : false;
            maze.isSilceableY = maze.heigth > minMazeSizeY * 2 ? true : false;
            if (!maze.isSilceableX && !maze.isSilceableY)
                return;

            if (maze.isSilceableX && maze.isSilceableY) {
                int iAxisToCut = Random.Range(0, 2);
                if (iAxisToCut == 0) {
                    maze.isSilceableX = false;
                }
                else {
                    maze.isSilceableY = false;
                }
            }

            if (maze.isSilceableX && !maze.isSilceableY) {
                int RandomCut = Random.Range(maze.minX + minMazeSizeX + 1, maze.maxX - minMazeSizeX - 1);
                //if (((maze.maxX - minMazeSizeX - 1) - (maze.minX + minMazeSizeX + 1) == 1) && maze.lastCutType == Maze.LastCutType.horizontal) {
                //    return;
                //}
                //do {
                //    RandomCut = Random.Range(maze.minX + minMazeSizeX + 1, maze.maxX - minMazeSizeX - 1);
                //    Debug.Log($"{maze.lastRandomBridge}, {RandomCut}");
                //}
                //while (maze.lastCutType == Maze.LastCutType.horizontal && maze.lastRandomBridge == RandomCut);
                for (int i = maze.minY; i <= maze.maxY; i++) {
                    if (_matrix[RandomCut, i].transform.position.x == RandomCut 
                        && _matrix[RandomCut, i].transform.position.z >= maze.minY 
                        && _matrix[RandomCut, i].transform.position.z <= maze.maxY ) {
                        _allModulesInScene.Remove(_matrix[RandomCut, i]);
                        DestroyImmediate(_matrix[RandomCut, i].gameObject);
                        SetNeighbors();
                    }
                }

                int RandomBridge = Random.Range(maze.minY, maze.maxY);
                GameObject tempObj = Instantiate(_modulePrefab);
                tempObj.transform.parent = transform;
                tempObj.transform.position = new Vector3(RandomCut, _modulePrefab.transform.position.y, RandomBridge);
                _allModulesInScene.Add(tempObj.GetComponent<Module>());
                _matrix[RandomCut, RandomBridge] = tempObj.GetComponent<Module>();
                SetNeighbors();

                Maze maze1 = new Maze() {
                    minX = maze.minX,
                    maxX = RandomCut - 1,
                    minY = maze.minY,
                    maxY = maze.maxY,
                    lastRandomBridge = RandomBridge,
                    lastCutType = Maze.LastCutType.vertical
                };
                Maze maze2 = new Maze() {
                    minX = RandomCut + 1,
                    maxX = maze.maxX,
                    minY = maze.minY,
                    maxY = maze.maxY,
                    lastRandomBridge = RandomBridge,
                    lastCutType = Maze.LastCutType.vertical
                };
                BinarySpacePartition(maze1);
                BinarySpacePartition(maze2);
            }

            if (!maze.isSilceableX && maze.isSilceableY) {
                int RandomCut = Random.Range(maze.minY + minMazeSizeY + 1, maze.maxY - minMazeSizeY - 1);
                //if (((maze.maxY - minMazeSizeY - 1) - (maze.minY + minMazeSizeY + 1) == 1) && maze.lastCutType == Maze.LastCutType.vertical) {
                //    return;
                //}
                //do {
                //    RandomCut = Random.Range(maze.minY + minMazeSizeY + 1, maze.maxY - minMazeSizeY - 1);
                //    Debug.Log($"{maze.lastRandomBridge}, {RandomCut}");
                //}
                //while (maze.lastCutType == Maze.LastCutType.vertical && maze.lastRandomBridge == RandomCut);
                for (int i = maze.minX; i <= maze.maxX; i++) {
                    if (_matrix[i, RandomCut].transform.position.z == RandomCut 
                        && _matrix[i, RandomCut].transform.position.x >= maze.minX 
                        && _matrix[i, RandomCut].transform.position.x <= maze.maxX) {
                        _allModulesInScene.Remove(_matrix[i, RandomCut]);
                        DestroyImmediate(_matrix[i, RandomCut].gameObject);
                        SetNeighbors();
                    }
                }

                int RandomBridge = Random.Range(maze.minY, maze.maxY);
                GameObject tempObj = Instantiate(_modulePrefab);
                tempObj.transform.parent = transform;
                tempObj.transform.position = new Vector3(RandomBridge, _modulePrefab.transform.position.y, RandomCut);
                _allModulesInScene.Add(tempObj.GetComponent<Module>());
                _matrix[RandomBridge, RandomCut] = tempObj.GetComponent<Module>();
                SetNeighbors();

                Maze maze1 = new Maze() {
                    minX = maze.minX,
                    maxX = maze.maxX,
                    minY = maze.minY,
                    maxY = RandomCut - 1,
                    lastRandomBridge = RandomBridge,
                    lastCutType = Maze.LastCutType.horizontal
                };
                Maze maze2 = new Maze() {
                    minX = maze.minX,
                    maxX = maze.maxX,
                    minY = RandomCut + 1,
                    maxY = maze.maxY,
                    lastRandomBridge = RandomBridge,
                    lastCutType = Maze.LastCutType.horizontal
                };
                BinarySpacePartition(maze1);
                BinarySpacePartition(maze2);
            }
        }
        #endregion
        #region LocalMethods
        bool RayHitLayout() {
            Debug.DrawRay(_rayFromSceneCamera.origin, _rayFromSceneCamera.direction * 10000f, Color.green, 5f);
            return Physics.Raycast(_rayFromSceneCamera, out _rayCastHit, 10000f) && _rayCastHit.collider.gameObject.layer == LayerMask.NameToLayer("Layout");
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
        #endregion
    }
}
