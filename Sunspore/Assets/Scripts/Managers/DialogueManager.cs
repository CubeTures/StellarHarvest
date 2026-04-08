using System.Collections;
using UnityEngine;

namespace Sunspore
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager _dm;
        public GameObject dialogue;

        void Awake()
        {
            if (_dm == null)
            {
                _dm = this.GetComponent<DialogueManager>();
            }
        }

        void Update()
        {
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
        }

        public bool GetEventStatus()
        {
            return dialogue.GetComponent<Dialogue>().isEvent;
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
                    if (PlayerPrefManager.GetInventoryOpenCount() >= 1)
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
    }
}
