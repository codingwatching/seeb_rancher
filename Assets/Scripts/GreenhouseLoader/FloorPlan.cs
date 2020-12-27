using Assets.Scripts.Tiling.TileSets;
using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader
{
    [RequireComponent(typeof(UniversalCoordinateSystemMembers))]
    [ExecuteInEditMode]
    public class FloorPlan : MonoBehaviour
    {
        public float tilingDistance;
        public RectCoordinateRange floorPlanSize;
        public FloorTiles floorTiles;
        private UniversalCoordinateSystemMembers tiles => GetComponent<UniversalCoordinateSystemMembers>();


        public void GenerateFloorPlan()
        {
            var generatedMembers = floorTiles.GenerateFloorPlan();
            tiles.SetupFromSaveObject(generatedMembers);
        }

        public float2 GetLocalPoint(UniversalCoordinate coord)
        {
            return coord.ToPositionInPlane() * tilingDistance;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}