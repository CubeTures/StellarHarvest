using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private List<TutorialStep> steps;
    private int currentStepIndex;

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

        if (currentStepIndex < steps.Count)
            StartStep();
        else
            Debug.Log("Tutorial Complete");
    }
}
