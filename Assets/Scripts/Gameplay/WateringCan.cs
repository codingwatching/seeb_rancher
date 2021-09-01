using Dman.LSystem.SystemRuntime.VolumetricData;
using Dman.LSystem.SystemRuntime.VolumetricData.Layers;
using Dman.LSystem.SystemRuntime.VolumetricData.NativeVoxels;
using Dman.ReactiveVariables;
using Environment;
using Simulation.VoxelLayers;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Gameplay
{
    public class WateringCan : MonoBehaviour
    {
        public RainToTerrainEffect rainEffect;
        public PerlinSampler terrainSampler;
        public OrganVolumetricWorld volumeWorld;

        public float waterFlowRate;
        public Vector2Int voxelWaterSize;
        public Transform wateringCanRenderer;
        public bool isWatering = true;

        public FloatVariable waterLeft;

        private JobHandle? wateringDep;

        private WateringCanEffect lastEffect;

        private void Awake()
        {
        }

        private void OnDestroy()
        {
            wateringDep?.Complete();
            wateringDep = null;
        }

        // Update is called once per frame
        void Update()
        {
            var layout = volumeWorld.VoxelLayout;
            var minTilePosition = GetMinWateringCoordinate();

            if (wateringDep.HasValue)
            {
                wateringDep.Value.Complete();
                wateringDep = null;
            }
            WateringCanEffect nextEffect;
            if (isWatering)
            {
                nextEffect = new WateringCanEffect
                {
                    minTile = minTilePosition,
                    maxTile = minTilePosition + voxelWaterSize,
                    waterAmountPerTile = waterFlowRate / (voxelWaterSize.x * voxelWaterSize.y)
                };
            }
            else
            {
                nextEffect = WateringCanEffect.Empty;
            }

            ApplyWateringCanEffect(nextEffect);

            var tileCenter = minTilePosition + (voxelWaterSize / 2);
            var tilePos = layout.SurfaceToCenterOfTile(tileCenter);
            var sampleAtPoint = terrainSampler.SampleNoise(tilePos);
            var pointInSpace = new Vector3(tilePos.x, sampleAtPoint, tilePos.y);

            wateringCanRenderer.position = pointInSpace;
        }

        private void OnDisable()
        {
            isWatering = false;
            ApplyWateringCanEffect(WateringCanEffect.Empty);
        }

        private void ApplyWateringCanEffect(WateringCanEffect nextEffect)
        {
            var layout = volumeWorld.VoxelLayout;

            if (lastEffect.waterAmountPerTile != 0 || nextEffect.waterAmountPerTile != 0)
            {
                var nativeSampler = terrainSampler.AsNativeCompatible();
                var rainData = rainEffect.GetRainAmountArrayWritable();

                var rainJob = new SquareWateringCanTileAssignmentJob
                {
                    rainFactorPerTile = rainData,
                    layout = layout,
                    lastEffect = lastEffect,
                    currentEffect = nextEffect,
                };
                wateringDep = rainJob.Schedule(layout.totalTiles, 100, rainEffect.GetDependencyNeededToWrite());

                rainEffect.RegisterWritingDependencyOnRainAmount(wateringDep.Value);
                nativeSampler.Dispose(wateringDep.Value);

                waterLeft?.Add(-nextEffect.TotalWaterAmount() * Time.deltaTime);

                lastEffect = nextEffect;
            }
        }

        private void OnDrawGizmosSelected()
        {
            var minCenter = GetMinWateringCoordinate();
            var layout = volumeWorld.VoxelLayout;

            Gizmos.color = Color.blue;
            for (int x = 0; x < voxelWaterSize.x; x++)
            {
                for (int y = 0; y < voxelWaterSize.y; y++)
                {
                    var tileCoord = minCenter + new Vector2Int(x, y);
                    var tilePos = layout.SurfaceToCenterOfTile(tileCoord);
                    var sampleAtPoint = terrainSampler.SampleNoise(tilePos);

                    var pointInSpace = new Vector3(tilePos.x, sampleAtPoint, tilePos.y);
                    Gizmos.DrawSphere(pointInSpace, 0.1f);

                    var voxelCoord = layout.GetVoxelCoordinates(pointInSpace);
                    var voxelCenter = layout.CoordinateToCenterOfVoxel(voxelCoord);
                    Gizmos.DrawWireCube(voxelCenter, layout.voxelSize);
                }
            }
        }

        private Vector2Int GetMinWateringCoordinate()
        {
            var layout = volumeWorld.VoxelLayout;
            var tilePosition = layout.SurfaceGetSurfaceCoordinates(transform.position) - (voxelWaterSize / 2);
            tilePosition.Clamp(Vector2Int.zero, new Vector2Int(layout.worldResolution.x, layout.worldResolution.z) - voxelWaterSize);
            return tilePosition;
        }


        struct WateringCanEffect
        {
            public static readonly WateringCanEffect Empty = new WateringCanEffect
            {
                minTile = new Vector2Int(0, 0),
                maxTile = new Vector2Int(0, 0),
                waterAmountPerTile = 0
            };

            public Vector2Int minTile;
            public Vector2Int maxTile;
            public float waterAmountPerTile;
            public bool ContainsTile(Vector2Int tileCoordiante)
            {
                return (tileCoordiante.x >= minTile.x && tileCoordiante.x < maxTile.x) &&
                    (tileCoordiante.y >= minTile.y && tileCoordiante.y < maxTile.y);
            }

            public float TotalWaterAmount()
            {
                var size = maxTile - minTile;
                return (size.x * size.y * waterAmountPerTile);
            }
        }

        struct SquareWateringCanTileAssignmentJob : IJobParallelFor
        {
            public NativeArray<float> rainFactorPerTile;

            public VolumetricWorldVoxelLayout layout;

            public WateringCanEffect currentEffect;
            public WateringCanEffect lastEffect;

            public void Execute(int index)
            {
                var tileIndex = new TileIndex(index);
                var tileCoordinate = layout.SurfaceGetCoordinatesFromTileIndex(tileIndex);

                if (currentEffect.ContainsTile(tileCoordinate))
                {
                    rainFactorPerTile[tileIndex.Value] += currentEffect.waterAmountPerTile;
                }
                if (lastEffect.ContainsTile(tileCoordinate))
                {
                    rainFactorPerTile[tileIndex.Value] -= lastEffect.waterAmountPerTile;
                }
            }
        }
    }
}
