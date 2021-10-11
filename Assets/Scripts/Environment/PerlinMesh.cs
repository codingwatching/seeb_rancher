using ProceduralToolkit;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Environment
{
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class PerlinMesh : MonoBehaviour
    {
        public PerlinSampler sampler;
        /// <summary>
        /// the total number of samples in each dimension to take across this tile
        /// </summary>
        public Vector2Int samplesPerTile = 5 * Vector2Int.one;
        public Vector2 size = Vector2.one;

        private void Awake()
        {
            sampler.onNoiseFieldChanged += RegenMesh;
        }

        private void Start()
        {
            RegenMesh();
        }
        private void OnDestroy()
        {
            sampler.onNoiseFieldChanged -= RegenMesh;
        }
        private void OnValidate()
        {
            RegenMesh();
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct MeshVertexLayout
        {
            public float3 pos;
            public float3 normal;
            public MeshVertexLayout(float3 pos)
            {
                this.pos = pos;
                this.normal = default;
            }
        }
        private static VertexAttributeDescriptor[] GetVertexLayout()
        {
            return new[]{
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32),
                };
        }

        private void RegenMesh()
        {
            using var vertexes = new NativeArray<MeshVertexLayout>(samplesPerTile.x * samplesPerTile.y * BuildMeshData.VertexesPerSample, Allocator.TempJob);
            using var indexes = new NativeArray<int>(samplesPerTile.x * samplesPerTile.y * BuildMeshData.IndexesPerSample, Allocator.TempJob);
            using var nativeSampler = sampler.AsNativeCompatible(Allocator.TempJob);

            var nativeRandomSampler = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

            var meshbuildJob = new BuildMeshData
            {
                vertexes = vertexes,
                indexes = indexes,
                nativeSampler = nativeSampler,

                localTransform = transform.localToWorldMatrix,
                tileResolution = samplesPerTile,
                quadSize = new Vector2(size.x / samplesPerTile.x, size.y / samplesPerTile.y),
                randomSampler = nativeRandomSampler,

            };

            var dep = meshbuildJob.Schedule(samplesPerTile.x * samplesPerTile.y, 1000);
            dep.Complete();

            var meshfilter = GetComponent<MeshFilter>();

            var mesh = meshfilter.sharedMesh;
            mesh.Clear();
            mesh.SetVertexBufferParams(vertexes.Length, GetVertexLayout());
            mesh.SetVertexBufferData(vertexes, 0, 0, vertexes.Length, 0, MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);

            mesh.SetIndexBufferParams(indexes.Length, IndexFormat.UInt32);
            mesh.SetIndexBufferData(indexes, 0, 0, indexes.Length, MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices);

            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor
            {
                baseVertex = 0,
                topology = MeshTopology.Triangles,
                indexCount = indexes.Length,
                indexStart = 0
            });

            mesh.RecalculateNormals();


            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = mesh;
            }
        }

        private Vector3 VertexInPlane(float x, float y)
        {
            var samplePoint = transform.TransformPoint(x, 0, y);
            return new Vector3(x, sampler.SampleNoise(samplePoint.x, samplePoint.z), y);
        }

        [BurstCompile]
        struct BuildMeshData : IJobParallelFor
        {
            [NativeDisableParallelForRestriction]
            public NativeArray<MeshVertexLayout> vertexes;
            [NativeDisableParallelForRestriction]
            public NativeArray<int> indexes;
            [ReadOnly]
            public PerlinSamplerNativeCompatable nativeSampler;

            public Matrix4x4 localTransform;
            public Vector2Int tileResolution;
            public Vector2 quadSize;
            public Unity.Mathematics.Random randomSampler;

            public static readonly int VertexesPerSample = 6;
            public static readonly int IndexesPerSample = 6;

            public void Execute(int index)
            {
                var sampleIndex = index;
                var x = sampleIndex % tileResolution.x;
                var y = sampleIndex / tileResolution.x;
                var quadX = x * quadSize.x;
                var quadY = y * quadSize.y;
                var vertexOrigin = sampleIndex * VertexesPerSample;


                var x0y0 = VertexInPlane(quadX, quadY);
                var x1y0 = VertexInPlane(quadX + quadSize.x, quadY);
                var x0y1 = VertexInPlane(quadX, quadY + quadSize.y);
                var x1y1 = VertexInPlane(quadX + quadSize.x, quadY + quadSize.y);

                var indexOrigin = sampleIndex * IndexesPerSample;
                if (randomSampler.NextBool())
                {
                    vertexes[vertexOrigin + 0] = new MeshVertexLayout(x0y0);
                    indexes[indexOrigin + 0] = vertexOrigin + 0;
                    vertexes[vertexOrigin + 1] = new MeshVertexLayout(x0y1);
                    indexes[indexOrigin + 1] = vertexOrigin + 1;
                    vertexes[vertexOrigin + 2] = new MeshVertexLayout(x1y1);
                    indexes[indexOrigin + 2] = vertexOrigin + 2;

                    vertexes[vertexOrigin + 3] = new MeshVertexLayout(x1y1);
                    indexes[indexOrigin + 3] = vertexOrigin + 3;
                    vertexes[vertexOrigin + 4] = new MeshVertexLayout(x1y0);
                    indexes[indexOrigin + 4] = vertexOrigin + 4;
                    vertexes[vertexOrigin + 5] = new MeshVertexLayout(x0y0);
                    indexes[indexOrigin + 5] = vertexOrigin + 5;
                }
                else
                {
                    vertexes[vertexOrigin + 0] = new MeshVertexLayout(x0y1);
                    indexes[indexOrigin + 0] = vertexOrigin + 0;
                    vertexes[vertexOrigin + 1] = new MeshVertexLayout(x1y1);
                    indexes[indexOrigin + 1] = vertexOrigin + 1;
                    vertexes[vertexOrigin + 2] = new MeshVertexLayout(x1y0);
                    indexes[indexOrigin + 2] = vertexOrigin + 2;

                    vertexes[vertexOrigin + 3] = new MeshVertexLayout(x1y0);
                    indexes[indexOrigin + 3] = vertexOrigin + 3;
                    vertexes[vertexOrigin + 4] = new MeshVertexLayout(x0y0);
                    indexes[indexOrigin + 4] = vertexOrigin + 4;
                    vertexes[vertexOrigin + 5] = new MeshVertexLayout(x0y1);
                    indexes[indexOrigin + 5] = vertexOrigin + 5;
                }
            }

            private Vector3 VertexInPlane(float x, float y)
            {
                var samplePoint = localTransform.MultiplyPoint(new Vector3(x, 0, y));
                return new Vector3(x, nativeSampler.SampleNoise(new Vector2(samplePoint.x, samplePoint.z)), y);
            }

        }
    }
}
