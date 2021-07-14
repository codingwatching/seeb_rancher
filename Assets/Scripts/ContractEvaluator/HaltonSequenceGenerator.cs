using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.ContractEvaluator
{
    public class HaltonSequenceGenerator
    {
        private int horizontalRadix;
        private int verticalRadix;
        private int indexInSequence;

        private Vector2 offset;
        private Vector2 scale;

        public HaltonSequenceGenerator(int horizontalRadix, int verticalRadix, int seed, Vector2 maxVector, Vector2 minVector)
        {
            this.horizontalRadix = horizontalRadix;
            this.verticalRadix = verticalRadix;
            indexInSequence = seed;
            offset = minVector;
            scale = maxVector - minVector;
        }

        public IEnumerable<Vector2> InfiniteSample()
        {
            var i = 0;
            while (i++ < 1000)
            {
                yield return Sample();
            }
            Debug.LogError("MR Halton can't make this many points, it's too dangerous!");
        }

        public Vector2 Sample()
        {
            var sample = new Vector2(
                HaltonSequence.Get(indexInSequence, horizontalRadix),
                HaltonSequence.Get(indexInSequence, verticalRadix)
                );
            indexInSequence++;

            return (sample * scale) + offset;
        }
    }
}
