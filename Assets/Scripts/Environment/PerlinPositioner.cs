using UnityEngine;

namespace Environment
{
    [ExecuteInEditMode]
    public class PerlinPositioner : MonoBehaviour
    {
        public PerlinSampler sampler;
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
            var samplePoint = transform.TransformPoint(sampleOffsetFromTransformCenter.x, 0, sampleOffsetFromTransformCenter.y);
            position.y = sampler.SampleNoise(samplePoint.x, samplePoint.z) + yOffset;
            transform.position = position;
        }
    }
}
