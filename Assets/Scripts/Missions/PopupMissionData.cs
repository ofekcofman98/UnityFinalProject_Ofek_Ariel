using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tutorial Popup Mission", menuName = "SQL Detective/Mission/Tutorial Popup")]
public class TutorialPopupMissionData : MissionData
{
    [TextArea(3, 5)] public string popupText;
    public Sprite popupImage; // optional

    public override bool Validate()
    {
        return true; // Always valid once user clicks "Continue"
    }

    public override void ShowUI(MissionUIManager uiManager)
    {
        uiManager.ShowTutorialPopup(missionTitle, missionDescription, () =>
        {
            GameManager.Instance.missionManager.MarkMissionAsCompleted();
            CoroutineRunner.Instance.StartCoroutine(GameManager.Instance.missionManager.DelayedAdvance());
        });
    }

}
