using System.Collections;
using UnityEngine;

namespace Sunspore
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager _dm;
        public GameObject dialogue;
        private int _pendingCompleteEventCount;

        void Awake()
        {
            if (_dm == null)
            {
                _dm = this.GetComponent<DialogueManager>();
            }
        }

        void Update()
        {
            TryConsumePendingCompleteEvent();
            DialogueEvents();

            if (Input.GetKeyDown(KeyCode.L) && Application.isEditor)
            {
                PlayerPrefManager.SetEventCount(PlayerPrefManager.GetEventCount() + 1);
            }
        }

        public void CompleteEvent()
        {
            if (dialogue == null)
            {
                Debug.LogWarning("CompleteEvent called but dialogue reference is missing.");
                return;
            }

            Dialogue dialogueComponent = dialogue.GetComponent<Dialogue>();
            if (dialogueComponent == null)
            {
                Debug.LogWarning("CompleteEvent called but Dialogue component is missing.");
                return;
            }

            bool completed = dialogueComponent.CompleteEvent();
            if (completed)
            {
                PlayerPrefManager.SetEventCount(PlayerPrefManager.GetEventCount() + 1);
            }
            else
            {
                // If called too early, queue it and auto-complete once an event becomes active.
                _pendingCompleteEventCount++;
            }
        }

        public bool GetEventStatus()
        {
            if (dialogue == null)
            {
                return false;
            }

            Dialogue dialogueComponent = dialogue.GetComponent<Dialogue>();
            return dialogueComponent != null && dialogueComponent.isEvent;
        }

        public int GetEventCount()
        {
            return PlayerPrefManager.GetEventCount();
        }

        // All the dialogue events
        void DialogueEvents()
        {
            // Only check events if currently in an event state
            if (!GetEventStatus())
            {
                return;
            }

            int eventCount = PlayerPrefManager.GetEventCount();

            switch (eventCount)
            {
                case 0:
                    // Opening inventory
                    if (PlayerPrefManager.GetInventoryOpenCount() >= 1 || PlayerPrefManager.GetStoreOpenCount() >= 1)
                    {
                        // CompleteEvent();
                    }
                    break;
                    // case 1:
                    //     // Crafting something
                    //     if (PlayerPrefManager.GetCraftingCount() >= 1)
                    //     {
                    //         CompleteEvent();
                    //     }
                    //     break;
            }
        }

        private void TryConsumePendingCompleteEvent()
        {
            if (_pendingCompleteEventCount <= 0)
            {
                return;
            }

            if (dialogue == null)
            {
                return;
            }

            Dialogue dialogueComponent = dialogue.GetComponent<Dialogue>();
            if (dialogueComponent == null || !dialogueComponent.isEvent)
            {
                return;
            }

            bool completed = dialogueComponent.CompleteEvent();
            if (!completed)
            {
                return;
            }

            _pendingCompleteEventCount--;
            PlayerPrefManager.SetEventCount(PlayerPrefManager.GetEventCount() + 1);
        }
    }
}
