using Assets.Scripts.Plants;
using Assets.Scripts.Utilities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.PlantData
{
    public class PlantViewer: MonoBehaviour
    {
        public GameObjectVariable selectedPlant;


        private void Awake()
        {
            selectedPlant.Value
                .TakeUntilDestroy(this)
                .Subscribe(newObject =>
                {
                    ClearPlantViewer();
                    var plantContainer = newObject?.GetComponentInParent<PlantContainer>();
                    if (plantContainer?.plantType != null)
                    {
                        SetupPlantViewer(plantContainer);
                    }
                }).AddTo(this);
        }

        private void SetupPlantViewer(PlantContainer plantContainer)
        {
            foreach(Transform transform in plantContainer.plantsParent.transform)
            {
                Instantiate(transform.gameObject, this.transform);
            }
        }

        private void ClearPlantViewer()
        {
            gameObject.DestroyAllChildren();
        }
    }
}
