using Dman.ObjectSets;
using Dman.SceneSaveSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dman.NarrativeSystem
{
    [CreateAssetMenu(fileName = "Narrative", menuName = "Narrative/Narrative", order = 1)]
    public class GameNarrative : UniqueObjectRegistryWithAccess<Conversation>, ISaveableData
    {
        public HashSet<int> completedConversations;

        public string UniqueSaveIdentifier => "GameNarrative";

        private Queue<Conversation> activatedConversations;
        private Conversation currentlyActiveConvo = null;

        public void Init()
        {
            completedConversations = new HashSet<int>();
            activatedConversations = new Queue<Conversation>();
            currentlyActiveConvo = null;
        }

        public void CheckAllConversationTriggers()
        {
            foreach (var convo in allObjects)
            {
                if (!completedConversations.Contains(convo.myId) &&
                    !activatedConversations.Contains(convo) &&
                    currentlyActiveConvo != convo &&
                    convo.ShouldStartConversation(this))
                {
                    activatedConversations.Enqueue(convo);
                }
            }
            StartNextConvoIfExists();
        }

        private void StartNextConvoIfExists()
        {
            if(currentlyActiveConvo != null || activatedConversations.Count <= 0)
            {
                return;
            }
            var nextConvo = activatedConversations.Dequeue();
            if (nextConvo != null)
            {
                nextConvo.StartConversation(this);
                currentlyActiveConvo = nextConvo;
            }
        }

        public void ConversationEnded(Conversation endedConversation)
        {
            completedConversations.Add(endedConversation.myId);
            if(endedConversation != currentlyActiveConvo)
            {
                Debug.LogError("ended conversation which is not active.");
            }else
            {
                currentlyActiveConvo = null;
            }
            StartNextConvoIfExists();
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
                target.currentlyActiveConvo = null;
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
