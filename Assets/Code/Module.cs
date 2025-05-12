using UnityEngine;
using System.Collections.Generic;

namespace ProceduralLevelDesign {
    public class Module : MonoBehaviour {
        public bool northNeighbor;
        public bool southNeighbor;
        public bool eastNeighbor;
        public bool westNeighbor;

        public bool northEastNeighbor;
        public bool northWestNeighbor;
        public bool southEastNeighbor;
        public bool southWestNeighbor;

        [SerializeField] List<GameObject> walls;
        [SerializeField] List<GameObject> pillars;
        [SerializeField] public List<Module> neighbors;

        public void TurnOffWalls() {
            walls[0].SetActive(!northNeighbor);
            walls[1].SetActive(!southNeighbor);
            walls[2].SetActive(!eastNeighbor);
            walls[3].SetActive(!westNeighbor);
        }

        public void TurnOffPillars() {
            if (northNeighbor && eastNeighbor && northEastNeighbor) {
                pillars[0].SetActive(false);
            } else {
                pillars[0].SetActive(true);
            }
            if (northNeighbor && westNeighbor && northWestNeighbor) {
                pillars[1].SetActive(false);
            } else {
                pillars[1].SetActive(true);
            }
            if (southNeighbor && eastNeighbor && southEastNeighbor) {
                pillars[2].SetActive(false);
            } else {
                pillars[2].SetActive(true);
            }
            if (southNeighbor && westNeighbor && southWestNeighbor) {
                pillars[3].SetActive(false);
            } else {
                pillars[3].SetActive(true);
            }
        }
    }
}

