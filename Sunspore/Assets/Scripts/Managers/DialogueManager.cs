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
            // Check if dialogue GameObject is active before calling CompleteEvent
            if (!dialogue.activeInHierarchy)
            {
                dialogue.SetActive(true);
            }

            dialogue.GetComponent<Dialogue>().CompleteEvent();

            PlayerPrefManager.SetEventCount(PlayerPrefManager.GetEventCount() + 1);
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
                        CompleteEvent();
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
