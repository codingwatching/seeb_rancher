using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.GreenhouseLoader
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

            return sample / octaves.Sum(octave => octave.weight);
        }
        private void GenerateNoiseOffset()
        {
            var generator = new System.Random(randomSeed);
            noiseOffset = new Vector2(
                (float)generator.NextDouble() * 1e6f,
                (float)generator.NextDouble() * 1e6f
                );
        }

        private float SamplePerlin(Vector2 point, NoiseOctave octave)
        {
            var sampleVector = noiseOffset + Vector2.Scale(point, octave.frequency);
            return Mathf.PerlinNoise(sampleVector.x, sampleVector.y) * octave.weight;
        }
    }
}
