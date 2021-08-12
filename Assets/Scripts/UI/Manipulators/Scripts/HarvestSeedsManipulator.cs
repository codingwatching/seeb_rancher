using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using Assets.Scripts.UI.SeedInventory;
using Dman.LSystem.SystemRuntime.GlobalCoordinator;
using Dman.ReactiveVariables;
using Dman.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityFx.Outline;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    [CreateAssetMenu(fileName = "HarvestSeedsManipulator", menuName = "Tiling/Manipulators/HarvestSeedsManipulator", order = 2)]
    public class HarvestSeedsManipulator : MapManipulator, ISeedHoldingManipulator, IAreaSelectManipulator
    {

        public GameObjectVariable selectedGameObject;
        public RaycastGroup harvestCaster;
        public LayerMask dirtMask;
        public LayerMask haverstableMask;

        public Sprite harvestCursor;

        public GameObject dragAreaRenderer;

        private SeedBucketDisplay draggingSeedsInstance;
        private SeedBucket seeds = null;

        private ManipulatorController controller;
        private MovingOutlineHelper singleOutlineHelper;
        public OutlineLayerCollection outlineCollection;

        public string SeedGroupName => "harvested";

        public bool AttemptTransferAllSeedsInto(SeedBucket target)
        {
            if (target.TryTransferSeedsIntoSelf(seeds))
            {
                OnSeedsUpdated();
                return true;
            }
            return false;
        }
        public Seed[] AttemptTakeSeeds(int seedCount)
        {
            var resultSeeds = seeds?.TakeN(seedCount);
            if (resultSeeds != null)
            {
                OnSeedsUpdated();
            }
            return resultSeeds;
        }

        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
            selectedGameObject.SetValue(null);
            Debug.Log("harvest manipulator opened");
            CursorTracker.SetCursor(harvestCursor);
            seeds = new SeedBucket();
            singleOutlineHelper = new MovingOutlineHelper(outlineCollection);
        }

        public override void OnClose()
        {
            if (draggingSeedsInstance != null)
            {
                GameObject.Destroy(draggingSeedsInstance.gameObject);
            }
            draggingSeedsInstance = null;

            CursorTracker.ClearCursor();
            singleOutlineHelper.ClearOutlinedObject();
            selectedGameObject.SetValue(null);

            if (!seeds.Empty)
            {
                var seedReceiver = SeedInventoryController.Instance.CreateSeedStack(new SeedBucketUI
                {
                    bucket = seeds,
                    description = "Harvested"
                });
                if (seedReceiver == null)
                {
                    Debug.LogError($"No open seed slots, {seeds.SeedCount} seeds lost");
                }
            }
            seeds = null;
        }

        private Vector3? mouseDownPosition = null;

        public override bool OnUpdate()
        {
            var planter = GetHoveredPlantContainer();
            var validTarget = planter?.CanHarvest() ?? false;
            if (!isDragging)
            {
                // if draging is happening, don't update outline here.
                singleOutlineHelper.UpdateOutlineObject(validTarget ? planter.GetOutlineObject() : null);
            }


            if (!validTarget)
            {
                return true;
            }
            if (validTarget)
            {
                selectedGameObject.SetValue(planter.GetOutlineObject());
            }

            if (Input.GetMouseButtonDown(0))
            {
                mouseDownPosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0) && mouseDownPosition.HasValue)
            {
                var mouseDistance = (Input.mousePosition - mouseDownPosition).Value.magnitude;
                mouseDownPosition = null;
                if (mouseDistance < DragAreaSelector.MouseMoveDragThreshold)
                {
                    if (TryHarvestPlant(planter)) OnSeedsUpdated();
                }
            }

            return true;
        }

        private bool TryHarvestPlant(PlantedLSystem planter)
        {
            var harvested = planter.TryHarvest();
            if (harvested == null)
            {
                return false;
            }
            if (harvested.Length <= 0)
            {
                return true;
            }
            if (draggingSeedsInstance == null)
            {
                var draggingParentProvider = GameObject.FindObjectOfType<DraggingSeedSingletonProvider>();
                draggingSeedsInstance = draggingParentProvider.SpawnNewDraggingSeeds();
            }
            if (!seeds.TryAddSeedsToSet(harvested))
            {
                // TODO: what happens to the seeds if they can't be added? 
                //  we should probably not harvest the plant if the seeds will just dissapear into the void
                Debug.LogError("Incompatible seeds, they have been lost!");
                return false;
            }
            return true;
        }


        private PlantedLSystem GetHoveredPlantContainer()
        {
            var behavior = GlobalLSystemCoordinator.instance.GetBehaviorContainingOrganId(SelectedIdProvider.instance.HoveredId);

            // TODO: ensure l system behavior is in same game object as planted l system
            return behavior?.GetComponent<PlantedLSystem>();
        }

        private void OnSeedsUpdated()
        {
            if (seeds == null || seeds.Empty)
            {
                // close the manipulator if the bucket is empty
                controller.manipulatorVariable.SetValue(null);
                return;
            }
            draggingSeedsInstance.DisplaySeedBucket(seeds);
        }

        public void OnAreaSelected(Vector2 origin, Vector2 size)
        {
            // TODO: make sure that the planted L system prefabs have a primitive collider that
            //  can interact with this system
            Debug.Log("Harvest inside range:");
            Debug.Log(origin);
            Debug.Log(size);
            var harvestablePlanters = GetOverlappedPlanters(origin, size)
                .Where(x => x?.CanHarvest() ?? false)
                .ToList();
            foreach (var planter in harvestablePlanters)
            {
                if (!TryHarvestPlant(planter))
                {
                    return;
                }
            }
            OnSeedsUpdated();
        }

        private Vector3 lastCenter;
        private Vector3 lastExtents;
        private Quaternion lastRotation;
        public void OnDragAreaChanged(Vector2 origin, Vector2 size)
        {
            var harvestableSeeds = GetOverlappedPlanters(origin, size)
                .Where(x => x?.CanHarvest() ?? false)
                .Select(x => x.GetOutlineObject())
                .Where(x => x != null)
                .ToList();
            var newHighlight = new HashSet<GameObject>(harvestableSeeds);
            singleOutlineHelper.UpdateOutlineObjectSet(newHighlight);
        }


        private IEnumerable<PlantedLSystem> GetOverlappedPlanters(Vector2 mouseOrigin, Vector2 dragSize)
        {
            GetOverlappingBox(mouseOrigin, dragSize, out var center, out var halfExtents, out var rotation);

            center.y += 1;
            halfExtents.y += 1;

            lastCenter = center;
            lastExtents = halfExtents;
            lastRotation = rotation;

            var allTargetPlanters = Physics.OverlapBox(center, halfExtents, rotation, haverstableMask);
            return allTargetPlanters
                .Select(x => x.gameObject.GetComponentInParent<PlantedLSystem>());
        }

        private void GetOverlappingBox(Vector2 mouseOrigin, Vector2 dragSize, out Vector3 center, out Vector3 halfExtents, out Quaternion rotation)
        {
            var corners = new[]
            {
                mouseOrigin,
                mouseOrigin + new Vector2(dragSize.x, 0),
                mouseOrigin + new Vector2(dragSize.x, dragSize.y),
                mouseOrigin + new Vector2(0, dragSize.y),
            }.Select(
                x =>
                {
                    var ray = Camera.main.ScreenPointToRay(new Vector3(x.x, x.y, 0));
                    return ray;
                })
            .Select(x =>
            {
                if (Physics.Raycast(ray: x, hitInfo: out var hit, layerMask: dirtMask, maxDistance: 100f))
                {
                    return hit.point;
                }
                Debug.LogError("no raycast hit when dragging");
                return default;
            }).ToArray();

            var side1 = corners[1] - corners[0];
            side1.y = 0;
            rotation = Quaternion.FromToRotation(Vector3.forward, side1);

            var boxCenter = VectorUtils.Average(corners);

            var boxSpace = Matrix4x4.TRS(boxCenter, rotation, Vector3.one);
            var boxSpaceInverse = boxSpace.inverse;

            var cornersInBoxSpace = corners.Select(x => boxSpaceInverse.MultiplyPoint(x)).ToArray();

            var max = VectorUtils.VectorMax(cornersInBoxSpace);
            var min = VectorUtils.VectorMin(cornersInBoxSpace);
            center = ((max + min) / 2f) + boxCenter;
            halfExtents = (max - min) / 2f;
        }

        public override void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);

            if(lastRotation == default || lastCenter == default || lastExtents == default)
            {
                return;
            }

            var cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            Gizmos.DrawMesh(
                cube,
                lastCenter,
                lastRotation,
                lastExtents * 2);
        }

        private bool isDragging;
        public void SetDragging(bool isDragging)
        {
            this.isDragging = isDragging;
        }
    }
}
