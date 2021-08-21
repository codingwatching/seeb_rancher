using Dman.ReactiveVariables;
using Dman.SceneSaveSystem;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.MarketContracts.ChildCycler
{
    public interface IChildBuilder
    {
        public GameObject InstantiateUnderParent(GameObject parent);
    }

    /// <summary>
    /// Manages children objects which get replaced based on phase transitions
    /// </summary>
    public class PhaseBasedChildCyclingManager : MonoBehaviour
    {
        public BooleanReference generationEnabled;
        public IntReference levelPhase;
        [Range(0, 1)]
        public float chanceForNewChildPerPhase;
        public SaveablePrefabParent targetParent;
        public int maxChildren;

        private IChildBuilder builder;

        private void Awake()
        {
            builder = this.GetComponent<IChildBuilder>();

            levelPhase.ValueChanges
                .TakeUntilDestroy(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    if (!generationEnabled.CurrentValue)
                    {
                        return;
                    }
                    if (pair.Current - pair.Previous != 1)
                    {
                        return;
                    }
                    var hasNewContract = UnityEngine.Random.Range(0f, 1f) < chanceForNewChildPerPhase;
                    if (hasNewContract)
                    {
                        TriggerNewChild();
                    }
                }).AddTo(this);
        }

        public void TriggerNewChild()
        {
            var totalChildren = targetParent.transform.childCount;
            if (totalChildren < maxChildren)
            {
                this.builder.InstantiateUnderParent(targetParent.gameObject);
            }
            else
            {
                var indexToReplace = Random.Range(0, maxChildren);
                var transformToReplace = targetParent.transform.GetChild(indexToReplace);
                Destroy(transformToReplace.gameObject);
                var newObject = builder.InstantiateUnderParent(targetParent.gameObject);
                newObject.transform.SetSiblingIndex(indexToReplace);
            }
        }
    }
}
