using ProceduralToolkit;
using UnityEngine;

namespace Environment
{
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class PerlinMesh : MonoBehaviour
    {
        public PerlineSampler sampler;
        public Vector2Int samplesPerTile = 5 * Vector2Int.one;
        public Vector2 size = Vector2.one;

        private void Awake()
        {
            sampler.onNoiseFieldChanged += RegenMesh;
        }

        private void Start()
        {
            //RegenMesh();
        }
        private void OnDestroy()
        {
            sampler.onNoiseFieldChanged -= RegenMesh;
        }
        private void OnValidate()
        {
            //RegenMesh();
        }

        private void RegenMesh()
        {
            var randSampler = new System.Random(Random.Range(1, int.MaxValue));
            var quadWidth = size.x / samplesPerTile.x;
            var quadHeight = size.y / samplesPerTile.y;
            var builder = new MeshDraft();
            for (float x = 0; x < samplesPerTile.x; x++)
            {
                var quadX = x * quadWidth;
                for (float y = 0; y < samplesPerTile.y; y++)
                {
                    var quadY = y * quadHeight;
                    var x0y0 = VertexInPlane(quadX, quadY);
                    var x1y0 = VertexInPlane(quadX + quadWidth, quadY);
                    var x0y1 = VertexInPlane(quadX, quadY + quadHeight);
                    var x1y1 = VertexInPlane(quadX + quadWidth, quadY + quadHeight);
                    var center = VertexInPlane(quadX + quadWidth / 2, quadY + quadHeight / 2);
                    if (randSampler.NextDouble() > 0.5f)
                    {
                        builder.AddTriangle(x0y0, x0y1, x1y1, true);
                        builder.AddTriangle(x1y1, x1y0, x0y0, true);
                    }
                    else
                    {
                        builder.AddTriangle(x0y1, x1y1, x1y0, true);
                        builder.AddTriangle(x1y0, x0y0, x0y1, true);
                    }
                }
            }
            builder = builder.Paint(new Color(0, 0, 0, 0));
            var newMesh = builder.ToMesh(true, true);
            newMesh.RecalculateNormals();
            var meshfilter = GetComponent<MeshFilter>();
            meshfilter.mesh = newMesh;


            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = newMesh;
            }
        }

        private Vector3 VertexInPlane(float x, float y)
        {
            var samplePoint = transform.TransformPoint(x, 0, y);
            return new Vector3(x, sampler.SampleNoise(samplePoint.x, samplePoint.z), y);
        }
    }
}
