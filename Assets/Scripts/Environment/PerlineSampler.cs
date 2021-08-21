using System;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Environment
{
    [Serializable]
    public struct NoiseOctave
    {
        public Vector2 frequency;
        public float weight;
    }
    [CreateAssetMenu(fileName = "PerlinSampler", menuName = "Greenhouse/PerlinSampler", order = 2)]
    [ExecuteInEditMode]
    public class PerlineSampler : ScriptableObject
    {
        [SerializeField]
        private NoiseOctave[] octaves;
        [SerializeField]
        private float scale = 1;
        [SerializeField]
        private int randomSeed;
        [NonSerialized]
        private Vector2 noiseOffset = default;

        public event Action onNoiseFieldChanged;

        public void RandomizeNoise()
        {
            randomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            GenerateNoiseOffset();
            onNoiseFieldChanged?.Invoke();
        }
        private void OnValidate()
        {
            onNoiseFieldChanged?.Invoke();
        }

        public float SampleNoise(float x, float y)
        {
            return SampleNoise(new Vector2(x, y));
        }

        public float SampleNoise(Vector2 point)
        {
            if (noiseOffset == default)
            {
                GenerateNoiseOffset();
            }
            var perlinVector = point + noiseOffset;

            var sample = 0f;
            foreach (var octave in octaves)
            {
                sample += SamplePerlin(perlinVector, octave);
            }

            return scale * sample / octaves.Sum(octave => octave.weight);
        }
        private void GenerateNoiseOffset()
        {
            var generator = new System.Random(randomSeed);
            noiseOffset = new Vector2(
                (float)generator.NextDouble() * 10000f,
                (float)generator.NextDouble() * 10000f
                );
        }

        private float SamplePerlin(Vector2 point, NoiseOctave octave)
        {
            var sampleVector = noiseOffset + Vector2.Scale(point, octave.frequency);
            return (noise.srnoise(sampleVector, 0.6f) + 1) / 2f * octave.weight;
        }

        public PerlinSamplerNativeCompatable AsNativeCompatible(Allocator allocator = Allocator.Persistent)
        {
            return new PerlinSamplerNativeCompatable
            {
                octaves = new NativeArray<NoiseOctave>(octaves, allocator),
                scale = scale,
                noiseOffset = noiseOffset
            };
        }
    }

    public struct PerlinSamplerNativeCompatable : IDisposable, INativeDisposable
    {
        [ReadOnly]
        public NativeArray<NoiseOctave> octaves;
        public float scale;
        public Vector2 noiseOffset;

        public float SampleNoise(Vector2 point)
        {
            var perlinVector = point + noiseOffset;

            var sample = 0f;
            var totalWeight = 0f;
            for (int i = 0; i < octaves.Length; i++)
            {
                var octave = octaves[i];
                sample += SamplePerlin(perlinVector, octave);
                totalWeight += octave.weight;
            }
            return scale * sample / totalWeight;
        }

        private float SamplePerlin(Vector2 point, NoiseOctave octave)
        {
            var sampleVector = noiseOffset + Vector2.Scale(point, octave.frequency);
            return (noise.srnoise(sampleVector, 0.6f) + 1) / 2f * octave.weight;
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return octaves.Dispose(inputDeps);
        }

        public void Dispose()
        {
            octaves.Dispose();
        }
    }
}
