using Assets.Scripts.GreenhouseLoader;
using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.ReactiveVariables;
using UnityEngine;

namespace Assets.Scripts.PlantPathing
{
    public class SurfacePathingComponent : MonoBehaviour
    {
        public float waypointProximityRequirement = 0.1f;

        public float movementSpeed = 1f;
        public float patherHeight = 1.5f;
        public PerlineSampler terrainHeights;

        public float damageSpeed = 10f;
        public float ignorableDurability = 5f;
        public FloatReference gameSpeed;

        public string animationDamageFlagName = "damaging";
        public string animationMovementSpeedName = "moveSpeed";
        public string animationDeathName = "dead";
        public Animator animator;

        public float damageDoneByAttacking = 1f;
        public float health = 10f;
        public float timeToDie = 1f;
        private float diedTime = 0;

        private Vector3 nextWaypoint;
        private int nextVoxel = -1;
        private int minVoxelHeight;
        private int maxVoxelHeight;

        private PlantSurfacePathingWorld pathingWorld;
        private OrganDamageWorld damageWorld;
        private OrganVolumetricWorld durabilityWorld;
        private void Awake()
        {
            pathingWorld = GameObject.FindObjectOfType<PlantSurfacePathingWorld>();
            damageWorld = GameObject.FindObjectOfType<OrganDamageWorld>();
            durabilityWorld = GameObject.FindObjectOfType<OrganVolumetricWorld>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (health <= 0)
            {
                if (diedTime <= 0)
                {
                    animator.SetBool(animationDeathName, true);
                    diedTime = Time.time;
                }
                if (diedTime + (timeToDie / gameSpeed.CurrentValue) < Time.time)
                {
                    Destroy(gameObject);
                }
                return;
            }

            if (nextVoxel < 0 || (transform.position - nextWaypoint).magnitude < waypointProximityRequirement)
            {
                SetNextWaypoint(transform.position);
            }

            var deltaTime = Time.fixedDeltaTime * gameSpeed.CurrentValue;

            if (nextVoxel >= 0)
            {
                var layout = durabilityWorld.voxelLayout;
                transform.LookAt(nextWaypoint, Vector3.up);

                var durabilityData = durabilityWorld.nativeVolumeData.openReadData;
                var nextTileDurability = 0f;
                var tileCoordiante = layout.SurfaceGetCoordinatesFromDataIndex(nextVoxel);
                for (int y = minVoxelHeight; y <= maxVoxelHeight; y++)
                {
                    var voxel = new Vector3Int(tileCoordiante.x, y, tileCoordiante.y);
                    var id = layout.GetDataIndexFromCoordinates(voxel);
                    var durability = durabilityData[id];
                    nextTileDurability += durability;
                }

                if (nextTileDurability <= ignorableDurability)
                {
                    // move forward
                    var movementDir = nextWaypoint - transform.position;
                    transform.position += movementDir.normalized * movementSpeed * deltaTime;
                    animator.SetBool(animationDamageFlagName, false);
                    animator.SetFloat(animationMovementSpeedName, movementSpeed);
                }
                else
                {
                    var damagePerDurability = damageSpeed * deltaTime / nextTileDurability;
                    for (int y = minVoxelHeight; y <= maxVoxelHeight; y++)
                    {
                        var voxel = new Vector3Int(tileCoordiante.x, y, tileCoordiante.y);
                        var id = layout.GetDataIndexFromCoordinates(voxel);
                        var durability = durabilityData[id];

                        damageWorld.ApplyDamage(id, durability * damagePerDurability);
                    }
                    health -= damageDoneByAttacking * deltaTime;
                    //do damage
                    animator.SetBool(animationDamageFlagName, true);
                    animator.SetFloat(animationMovementSpeedName, 0f);
                }
            }

        }

        public void SetNextWaypoint(Vector3 currentPosition)
        {
            var volumeWorld = pathingWorld.volumeWorld;
            var layout = volumeWorld.voxelLayout;
            var voxel = layout.GetVoxelCoordinates(currentPosition);
            var positionIndex = layout.SurfaceGetDataIndexFromCoordinates(new Vector2Int(voxel.x, voxel.z));
            if (positionIndex < 0)
            {
                nextVoxel = -1;
                return;
            }
            var activeData = pathingWorld.activeParentNodeData;
            if (!activeData.HasValue)
            {
                nextVoxel = -1;
                return;
            }
            nextVoxel = activeData.Value[positionIndex];
            if (nextVoxel < 0)
            {
                return;
            }
            var tileWaypoint = layout.SurfaceGetTilePositionFromDataIndex(nextVoxel);
            var height = terrainHeights.SampleNoise(tileWaypoint.x, tileWaypoint.y);
            nextWaypoint = new Vector3(tileWaypoint.x, height, tileWaypoint.y);

            minVoxelHeight = Mathf.Max(Mathf.FloorToInt(height), 0);
            maxVoxelHeight = Mathf.Min(Mathf.CeilToInt(height + patherHeight), layout.worldResolution.y);
        }
    }
}