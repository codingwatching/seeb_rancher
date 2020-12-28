﻿using Assets.Scripts.Plants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.SeedInventory
{
    public class DraggingSeeds: MonoBehaviour
    {
        public Seed[] seedsInSet;
        public PlantTypeRegistry plantTypes;

        public Image seedSpriteDisplay;
        public TextMeshProUGUI seedCountDisplay;


        private void Awake()
        {
            seedsInSet = new Seed[0];
        }

        private void Update()
        {
            var mousePos = Input.mousePosition;
            this.transform.position = mousePos;
        }

        private void SetSeedSprite()
        {
            if(seedsInSet.Length <= 0)
            {
                seedSpriteDisplay.enabled = false;
            }else
            {
                var plantId = seedsInSet[0].plantType;
                var plantType = plantTypes.GetUniqueObjectFromID(plantId);
                seedSpriteDisplay.sprite = plantType.seedIcon;
            }
        }

        private void SetSeedCount()
        {
            seedCountDisplay.text = seedsInSet.Length.ToString();
        }

        public bool TryAddSeedsToSet(Seed[] seeds)
        {
            if (seedsInSet.Length <= 0)
            {
                seedsInSet = seeds;
                SetSeedSprite();
            }
            else
            {
                var seedType = seedsInSet[0].plantType;
                if (seeds.Any(s => s.plantType != seedType))
                {
                    return false;
                }
                this.seedsInSet = seedsInSet.Concat(seeds).ToArray();
            }
            this.SetSeedCount();
            return true;
        }
    }
}
