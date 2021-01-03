using Assets.Scripts.Utilities.Core;
using Assets.Scripts.Utilities.SaveSystem.Components;
using Assets.Scripts.Utilities.ScriptableObjectRegistries;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.NarrativeSystem
{
    [CreateAssetMenu(fileName = "Narrative", menuName = "Narrative/Narrative", order = 1)]
    public class GameNarrative : UniqueObjectRegistryWithAccess<Conversation>, ISaveableData
    {
        public HashSet<int> completedConversations;

        public string UniqueSaveIdentifier => "GameNarrative";

        private Queue<Conversation> activatedConversations;

        public void Init()
        {
            completedConversations = new HashSet<int>();
            activatedConversations = new Queue<Conversation>();
        }

        //private void Awake()
        //{
        //    completedConversations = new HashSet<int>();
        //}

        public void CheckAllConversationTriggers()
        {
            foreach (var convo in this.allObjects)
            {
                if (!completedConversations.Contains(convo.myId))
                {
                    if (convo.ShouldStartConversation(this))
                    {
                        activatedConversations.Enqueue(convo);
                    }
                }
            }
            StartNextConvoIfExists();
        }

        private void StartNextConvoIfExists()
        {
            if(activatedConversations.Count <= 0)
            {
                return;
            }
            var nextConvo = activatedConversations.Dequeue();
            if(nextConvo != null)
            {
                nextConvo.StartConversation(this);
            }
        }

        public void ConversationEnded(Conversation endedConversation)
        {
            completedConversations.Add(endedConversation.myId);
            this.StartNextConvoIfExists();
        }

        #region Saving
        [System.Serializable]
        class SavedNarrative
        {
            int[] completedConversations;
            public SavedNarrative(GameNarrative narrative)
            {
                completedConversations = narrative.completedConversations.ToArray();
            }

            public void ApplyTo(GameNarrative target)
            {
                target.completedConversations = new HashSet<int>(completedConversations);
            }
        }

        public ISaveableData[] GetDependencies()
        {
            return new ISaveableData[0];
        }

        public object GetSaveObject()
        {
            return new SavedNarrative(this);
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is SavedNarrative savedObj)
            {
                savedObj.ApplyTo(this);
            }
        }
        #endregion
    }
}
