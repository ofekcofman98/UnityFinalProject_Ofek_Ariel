using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStepReporter : MonoBehaviour
{
    [SerializeField] public string stepId;

    private void Awake()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => MissionsManager.Instance.ReportTutorialStep(stepId));
            Debug.Log($"[TutorialStepReporter]: {stepId}");
        }
    }
}
 