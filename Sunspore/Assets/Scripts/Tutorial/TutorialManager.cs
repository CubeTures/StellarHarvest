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
    bool stepActive = false;

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
        if (stepActive && currentStepIndex < steps.Count)
        {
            steps[currentStepIndex].Check();
        }
    }

    public void StartStep()
    {
        if (currentStepIndex >= steps.Count) return;
        stepActive = true;
        steps[currentStepIndex].Begin();
    }

    public void CompleteStep()
    {
        stepActive = false;
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
    }

    public void CompleteTutorial()
    {
        currentStepIndex = steps.Count;
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

    public void BroadcastSignal(Actions action)
    {
        if (!IsTutorialActive) return;
        if (onSignalSubscribers == null) return;
        foreach (OnSignal onSignal in onSignalSubscribers.GetInvocationList())
        {
            if (onSignal != null)
            {
                onSignal.Invoke(action);
            }
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
