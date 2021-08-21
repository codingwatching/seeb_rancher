using Simulation.Plants.PlantTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SeedInventory
{
    /// <summary>
    /// used to display a seed sprite and count, based on the data inside a <see cref="SeedBucket"/>
    /// </summary>
    public class SeedBucketDisplay : MonoBehaviour
    {
        public PlantTypeRegistry plantTypes;
        public Image seedSpriteDisplay;
        public TextMeshProUGUI seedCountDisplay;
        public void DisplaySeedBucket(SeedBucket bucket)
        {
            SetSeedSprite(bucket);
            SetSeedCount(bucket);
        }
        private void SetSeedSprite(SeedBucket myBucket)
        {
            if (myBucket.Empty)
            {
                seedSpriteDisplay.enabled = false;
            }
            else
            {
                seedSpriteDisplay.enabled = true;
                var plantId = myBucket.PlantTypeId;
                var plantType = plantTypes.GetUniqueObjectFromID(plantId);
                seedSpriteDisplay.sprite = plantType.seedIcon;
            }
        }

        private void SetSeedCount(SeedBucket myBucket)
        {
            var seedNum = myBucket.SeedCount;
            seedCountDisplay.text = seedNum > 0 ? seedNum.ToString() : "";
        }
    }
}