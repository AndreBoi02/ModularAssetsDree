using UnityEngine;
using System.Collections.Generic;

namespace ProceduralLevelDesign {
    public class Module : MonoBehaviour {
        #region RuntimeVars
        public bool northNeighbor { get; private set; }
        public bool southNeighbor { get; private set; }
        public bool eastNeighbor { get; private set; }
        public bool westNeighbor { get; private set; }

        public bool northEastNeighbor { get; private set; }
        public bool northWestNeighbor { get; private set; }
        public bool southEastNeighbor { get; private set; }
        public bool southWestNeighbor { get; private set; }
        #endregion
        #region References
        [SerializeField] List<GameObject> walls;
        [SerializeField] List<GameObject> pillars;
        [SerializeField] List<Module> neighbors;
        #endregion
        #region PublicMethods
        public void TurnOffWalls() {
            walls[0].SetActive(!northNeighbor);
            walls[1].SetActive(!southNeighbor);
            walls[2].SetActive(!eastNeighbor);
            walls[3].SetActive(!westNeighbor);
        }

        public void TurnOffPillars() {
            pillars[0].SetActive(!(northNeighbor && eastNeighbor && northEastNeighbor));
            pillars[1].SetActive(!(northNeighbor && westNeighbor && northWestNeighbor));
            pillars[2].SetActive(!(southNeighbor && eastNeighbor && southEastNeighbor));
            pillars[3].SetActive(!(southNeighbor && westNeighbor && southWestNeighbor));
        }
        #endregion
        #region Setters
        public void GetNeighbors(List<Module> t_neighbors) {
            neighbors = t_neighbors;
        }

        public void SetNorthNeigh(bool val) {
            northNeighbor = val;
        }

        public void SetSouthNeigh(bool val) {
            southNeighbor = val;
        }

        public void SetWestNeigh(bool val) {
            westNeighbor = val;
        }

        public void SetEastNeigh(bool val) {
            eastNeighbor = val;
        }

        public void SetNorthWestNeigh(bool val) {
            northWestNeighbor = val;
        }

        public void SetNorthEastNeigh(bool val) {
            northEastNeighbor = val;
        }

        public void SetSouthWestNeigh(bool val) {
            southWestNeighbor = val;
        }

        public void SetSouthEastNeigh(bool val) {
            southEastNeighbor = val;
        }
        #endregion
    }
}
