using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.ReactiveVariables;
using UnityEngine;

namespace Assets.Scripts.PlantPathing
{
    public class DamageWhenVoxelTileReachedComponent : MonoBehaviour
    {
        public float waypointProximityRequirement = 0.1f;
        public float damageAmount;
        public FloatVariable damageTarget;

        public Vector2Int surfacePoint;

        private OrganVolumetricWorld durabilityWorld;
        private void Awake()
        {
            durabilityWorld = GameObject.FindObjectOfType<OrganVolumetricWorld>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            var voxelLayout = durabilityWorld.voxelLayout;
            var tilePoint = voxelLayout.SurfaceToCenterOfTile(surfacePoint);

            var currentSurfacePoint = new Vector2(transform.position.x, transform.position.z);

            var diff = currentSurfacePoint - tilePoint;
            if (diff.magnitude < waypointProximityRequirement)
            {
                damageTarget.Add(-damageAmount);
                Destroy(this.gameObject);
            }
        }
    }
}