using Dman.LSystem.SystemRuntime.VolumetricData;
using UnityEngine;

namespace Assets.Scripts.PlantPathing
{
    public class PathingComponent : MonoBehaviour
    {
        public float waypointProximityRequirement = 0.1f;

        public float movementSpeed = 1f;

        public float damageSpeed = 10f;
        public float ignorableDurability = 5f;

        public string animationDamageFlagName = "damaging";
        public string animationMovementSpeedName = "moveSpeed";
        public Animator animator;


        private Vector3 nextWaypoint;
        private int nextVoxel = -1;

        private PlantPathingWorld pathingWorld;
        private OrganDamageWorld damageWorld;
        private OrganVolumetricWorld durabilityWorld;
        private void Awake()
        {
            pathingWorld = GameObject.FindObjectOfType<PlantPathingWorld>();
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
            if(nextVoxel < 0 || (transform.position - nextWaypoint).magnitude < waypointProximityRequirement)
            {
                SetNextWaypoint(transform.position);
            }

            if (nextVoxel >= 0)
            {
                transform.LookAt(nextWaypoint, Vector3.up);

                var durabilityOfNextVoxel = durabilityWorld.nativeVolumeData.openReadData[nextVoxel];
                if(durabilityOfNextVoxel <= ignorableDurability)
                {
                    // move forward
                    var movementDir = nextWaypoint - transform.position;
                    transform.position += movementDir.normalized * movementSpeed * Time.fixedDeltaTime;
                    animator.SetBool(animationDamageFlagName, false);
                    animator.SetFloat(animationMovementSpeedName, movementSpeed);
                }
                else
                {
                    //do damage
                    damageWorld.ApplyDamage(nextVoxel, damageSpeed * Time.fixedDeltaTime);
                    animator.SetBool(animationDamageFlagName, true);
                    animator.SetFloat(animationMovementSpeedName, 0f);
                }
            }
        }

        public void SetNextWaypoint(Vector3 currentPosition)
        {
            var volumeWorld = pathingWorld.volumeWorld;
            var positionIndex = volumeWorld.voxelLayout.GetDataIndexFromWorldPosition(currentPosition);
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
            nextWaypoint = volumeWorld.voxelLayout.GetWorldPositionFromDataIndex(nextVoxel);
        }
    }
}