using Assets.Scripts.ContractEvaluator;
using Assets.Scripts.DataModels;
using Assets.Scripts.GreenhouseLoader;
using Assets.Scripts.UI.Manipulators.Scripts;
using Dman.LSystem.SystemRuntime.DOTSRenderer;
using Dman.LSystem.UnityObjects;
using Dman.ObjectSets;
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
    [RequireComponent(typeof(LSystemBehavior))]
    [RequireComponent(typeof(MeshFilter))]
    public class DestroyLSystemWhenEmpty : MonoBehaviour
    {
        public GameObject prefabRoot;

        public int maximumConsecutiveEmptyUpdates = 10;

        private int numberOfEmptyUpdates = 0;

        private void Awake()
        {
            this.GetComponent<LSystemBehavior>().OnSystemStateUpdated += SystemWasUpdated;
        }

        private void OnDestroy()
        {
            this.GetComponent<LSystemBehavior>().OnSystemStateUpdated -= SystemWasUpdated;
        }

        private void SystemWasUpdated()
        {
            var mesh = GetComponent<MeshFilter>();
            if(mesh.mesh.vertexCount < 5)
            {
                numberOfEmptyUpdates++;
            }else
            {
                numberOfEmptyUpdates = 0;
            }
            if(numberOfEmptyUpdates >= maximumConsecutiveEmptyUpdates)
            {
                Destroy(prefabRoot);
            }
        }

    }
}
