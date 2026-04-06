using System.Collections.Generic;
using Sunspore;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField]
    private List<TutorialStep> steps;
    private int currentStepIndex;

    /// <summary>True while there is an active step; false after the last step completes.</summary>
    public bool IsTutorialActive => steps != null && currentStepIndex < steps.Count;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartStep();
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
        if (DialogueManager._dm != null)
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
