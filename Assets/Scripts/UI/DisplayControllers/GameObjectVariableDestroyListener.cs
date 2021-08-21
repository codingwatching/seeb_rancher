using Dman.ReactiveVariables;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace UI.DisplayControllers
{
    public class GameObjectVariableDestroyListener : MonoBehaviour
    {
        public GameObjectVariable gameObjectVariable;
        private System.IDisposable targetDestroySubscriber = null;

        private void Awake()
        {
            gameObjectVariable.Value
                .TakeUntilDestroy(this)
                .Subscribe(obj =>
                {
                    targetDestroySubscriber?.Dispose();
                    targetDestroySubscriber = null;
                    if (obj == null) return;
                    targetDestroySubscriber = obj.OnDestroyAsObservable()
                        .Subscribe(x =>
                        {
                            gameObjectVariable.CurrentValue = null;
                        });
                }).AddTo(this);
        }

        private void OnDestroy()
        {
            targetDestroySubscriber?.Dispose();
        }
    }
}