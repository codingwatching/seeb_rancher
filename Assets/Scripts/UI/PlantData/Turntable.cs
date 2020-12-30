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
    public class Turntable: MonoBehaviour
    {
        public float rotationSpeed;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
