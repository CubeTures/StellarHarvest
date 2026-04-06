using System.Collections.Generic;
using Sunspore;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField]
    private List<TutorialStep> steps;
    private int currentStepIndex;
    [HideInInspector]
    public int TutorialStepLength;

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
            StartStep();
        else
            Debug.Log("Tutorial Complete");
    }
}
