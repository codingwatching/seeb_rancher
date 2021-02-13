using Assets.Scripts.DataModels;
using Assets.Scripts.Plants;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SeedInventory
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
            if (myBucket.AllSeeds.Length <= 0)
            {
                seedSpriteDisplay.enabled = false;
            }
            else
            {
                seedSpriteDisplay.enabled = true;
                var plantId = myBucket.AllSeeds[0].plantType;
                var plantType = plantTypes.GetUniqueObjectFromID(plantId);
                seedSpriteDisplay.sprite = plantType.seedIcon;
            }
        }

        private void SetSeedCount(SeedBucket myBucket)
        {
            var seedNum = myBucket.AllSeeds.Length;
            seedCountDisplay.text = seedNum > 0 ? seedNum.ToString() : "";
        }
    }
}