using System;
using System.Collections.Generic;
using Sunspore;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Serializable]
    public enum Actions
    {
        HOE,
        WATERING_CAN,
        SCYTHE,
        HOTBAR,
        PLANT,
        DEBT,
    }

    public delegate void OnSignal(Actions action);
    public OnSignal onSignalSubscribers;

    public static TutorialManager Instance;

    [SerializeField]
    private List<TutorialStep> steps;
    private int currentStepIndex;
    [HideInInspector]
    public int TutorialStepLength;
    public Dictionary<string, int> availableActions = new();

    /// <summary>True while there is an active step; false after the last step completes.</summary>
    public bool IsTutorialActive => steps != null && currentStepIndex < steps.Count;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TutorialStepLength = steps.Count;
        // if (PlayerPrefManager.GetTutorialStep() >= TutorialStepLength)
        // {
        //     Debug.Log("Tutorial completed automatically");
        //     return;
        // }
        // else
        // {
        StartStep();
        // }
    }

    void Update()
    {
        if (currentStepIndex < steps.Count)
        {
            steps[currentStepIndex].Check();
        }
    }

    private void StartStep()
    {
        steps[currentStepIndex].Begin();
    }

    public void CompleteStep()
    {
        steps[currentStepIndex].End();
        currentStepIndex++;
        PlayerPrefManager.SetTutorialStep(currentStepIndex);

        if (DialogueManager._dm == null)
        {
            Debug.LogError("DialogueManager not found");
        }
        else
        {
            DialogueManager._dm.CompleteEvent();
        }

        Debug.Log("Tutorial Step: " + currentStepIndex);

        if (currentStepIndex < steps.Count)
        {
            StartStep();
        }
        else
        {
            Debug.Log("Tutorial Complete");
            if (DayNightManager.Instance != null)
            {
                DayNightManager.Instance.startTime();
            }
            else
            {
                Debug.LogWarning("Tutorial complete but DayNightManager.Instance is null.");
            }
        }
    }

    public void BroadcastSignal(Actions action)
    {
        if (!IsTutorialActive) return;
        foreach (OnSignal onSignal in onSignalSubscribers.GetInvocationList())
        {
            onSignal.Invoke(action);
        }
    }

    public void SubscribeBroadcast(OnSignal onSignal)
    {
        onSignalSubscribers += onSignal;
    }

    public void UnsubscribeBroadcast(OnSignal onSignal)
    {
        onSignalSubscribers -= onSignal;
    }
}
