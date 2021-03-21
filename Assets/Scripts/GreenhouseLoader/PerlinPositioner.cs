using ProceduralToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader
{
    [ExecuteInEditMode]
    public class PerlinPositioner: MonoBehaviour
    {
        public PerlineSampler sampler;
        public Vector2 sampleOffsetFromTransformCenter = Vector2.zero;
        public float yOffset = 0f;
        private void Awake()
        {
            RepositionSelf();
            sampler.onNoiseFieldChanged += RepositionSelf;
        }
        private void OnDestroy()
        {
            sampler.onNoiseFieldChanged -= RepositionSelf;
        }
        private void OnValidate()
        {
            RepositionSelf();
        }

        private void RepositionSelf()
        {
            var position = transform.position;
            var samplePoint = this.transform.TransformPoint(sampleOffsetFromTransformCenter.x, 0, sampleOffsetFromTransformCenter.y);
            position.y = sampler.SampleNoise(samplePoint.x, samplePoint.z) + yOffset;
            transform.position = position;
        }
    }
}
