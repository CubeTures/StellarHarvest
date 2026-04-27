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
        INVENTORY,
    }

    public delegate void OnSignal(Actions action);
    public OnSignal onSignalSubscribers;

    public static TutorialManager Instance;

    [SerializeField]
    private List<TutorialStep> steps;
    private int currentStepIndex;
    [HideInInspector]
    public int TutorialStepLength;
    public Dictionary<Actions, bool> availableActions = new Dictionary<Actions, bool>
    {
        {Actions.HOE, false},
        {Actions.WATERING_CAN, false},
        {Actions.SCYTHE, false},
        {Actions.HOTBAR, false},
        {Actions.PLANT, false},
        {Actions.DEBT, false},
        {Actions.INVENTORY, false},
    };

    /// <summary>True while there is an active step; false after the last step completes.</summary>
    public bool IsTutorialActive => steps != null && currentStepIndex < steps.Count;
    bool stepActive = false;

    /// <summary>
    /// Set when a plant is successfully placed while the current step is not the PLANT <see cref="SignalStep"/>,
    /// so the PLANT signal was not consumed (e.g. planted during tilling / HOE step).
    /// </summary>
    bool pendingPlantSatisfiedForNextPlantStep;

    [SerializeField] private GameObject cratePrefab;
    [SerializeField] private Vector3 crateSpawnPoint = new Vector3(0, 100, -100);
    Quaternion crateRotation;

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
        availableActions[Actions.INVENTORY] = true;
        steps[currentStepIndex].Begin();
    }

    public void CompleteStep()
    {
        availableActions[Actions.INVENTORY] = false;
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

        // Spawn crate
        crateSpawnPoint += new Vector3(
            UnityEngine.Random.Range(-5, 5),
            1,
            UnityEngine.Random.Range(-5, 5)
        );
        crateRotation = Quaternion.Euler(
            UnityEngine.Random.Range(0, 360),
            UnityEngine.Random.Range(0, 360),
            UnityEngine.Random.Range(0, 360)
        );
        Instantiate(cratePrefab, crateSpawnPoint, crateRotation);

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

    /// <summary>Call from <see cref="PlacementManager"/> when a plant is placed, before <see cref="BroadcastSignal(Actions)"/>(PLANT).</summary>
    public void OnSuccessfulPlantBeforePlantSignal()
    {
        if (!IsTutorialActive) return;
        if (steps == null || currentStepIndex < 0 || currentStepIndex >= steps.Count) return;
        if (steps[currentStepIndex] is SignalStep s && s.ExpectedAction == Actions.PLANT)
            return;
        pendingPlantSatisfiedForNextPlantStep = true;
    }

    /// <summary>Used by the PLANT <see cref="SignalStep"/> in <c>Begin</c> to skip the step if the player already planted.</summary>
    public bool ConsumePendingPlantForPlantStep()
    {
        if (!pendingPlantSatisfiedForNextPlantStep) return false;
        pendingPlantSatisfiedForNextPlantStep = false;
        return true;
    }
}
