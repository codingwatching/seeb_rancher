using Assets.Scripts.Tiling.TileSets;
using Assets.Scripts.Utilities.SaveSystem;
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

        public int layer;

        private void Awake()
        {
            SaveSystemHooks.Instance.PostLoad += DataLoaded;
        }
        private void OnDestroy()
        {
            SaveSystemHooks.Instance.PostLoad -= DataLoaded;
        }

        private void DataLoaded()
        {
            RebuildTiles();
        }

        // Start is called before the first frame update
        void Start()
        {
            //RebuildTiles();
        }

        public void RebuildTiles()
        {
            gameObject.DestroyAllChildren(x => x.layer == layer);
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
            var newtile = type.CreateTile(coord, position, transform, tiles);
            newtile.layer = layer;
            return newtile;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}