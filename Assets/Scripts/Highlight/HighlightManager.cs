using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HighlightManager : Singleton<HighlightManager>
{
    private List<IHighlightable> currentHighlights = new();

    public void HighlightTutorialStep(MissionData mission)
    {
        ClearAllHighlights();

        if (!mission.isTutorial) { return; }

        if (mission is InteractableMissionData im)
        {
            InteractableObject interactableObject = FindObjectsOfType<InteractableObject>()
                .FirstOrDefault(o => o.InteractableId == im.requiredObjectId);

            IHighlightable highlightable = interactableObject?.GetComponent<IHighlightable>();
            RegisterHighlight(highlightable);
            
            if (highlightable is Highlightable concrete)
            {
                concrete.SetMarkerLabel(interactableObject.name);
            }

        }
    }

    public void RegisterHighlight(IHighlightable highlightable)
    {
        if (highlightable != null)
        {
            highlightable.Highlight(true);
            currentHighlights.Add(highlightable);
        }
    }

    public void ClearAllHighlights()
    {
        foreach (IHighlightable highlightable in currentHighlights)
        {
            highlightable.Highlight(false);
        }

        currentHighlights.Clear();
    }
}
