using Assets.Scripts.Tiling.TileSets;
using Assets.Tiling;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader
{
    [RequireComponent(typeof(UniversalCoordinateSystemMembers))]
    [RequireComponent(typeof(FloorPlan))]
    public class GreenhouseBuilder : MonoBehaviour
    {
        private FloorPlan floorPlan => GetComponent<FloorPlan>();
        private UniversalCoordinateSystemMembers tiles => GetComponent<UniversalCoordinateSystemMembers>();

        private void Awake()
        {
            floorPlan.GenerateFloorPlan();
        }

        // Start is called before the first frame update
        void Start()
        {
            foreach (var floorCoordinate in floorPlan.floorPlanSize)
            {
                CreateTileAt(UniversalCoordinate.From(floorCoordinate));
            }
        }


        public GameObject CreateTileAt(UniversalCoordinate coord)
        {
            var type = tiles.GetTileType(coord);
            if (type == null)
            {
                return null;
            }
            var position = floorPlan.GetLocalPoint(coord);
            return type.CreateTile(coord, position, transform, tiles);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}