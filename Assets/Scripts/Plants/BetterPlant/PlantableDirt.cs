using Assets.Scripts.DataModels;
using Assets.Scripts.GreenhouseLoader;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.LSystem.SystemRuntime.DOTSRenderer;
using Dman.LSystem.UnityObjects;
using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using Dman.Utilities;
using Genetics.GeneticDrivers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.Scripts.Plants
{
    /// <summary>
    /// marks a mesh with a collider as something which can be planted onto
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlantableDirt : MonoBehaviour, IManipulatorClickReciever
    {
        public GameObject GetOutlineObject()
        {
            return gameObject;
        }

        public bool IsSelectable()
        {
            return false;
        }

        public bool SelfHit(RaycastHit hit)
        {
            return false;
        }
    }
}
