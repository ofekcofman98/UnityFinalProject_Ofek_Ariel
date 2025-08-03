using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    public string GetBestDialogue(PersonData person)
    {
        MissionSequence sequence = MissionsManager.Instance.MissionSequence;
        if (sequence == null) return "…";

        PersonDialogueSet set = sequence.PersonDialogues.FirstOrDefault(d => d.personId == person.id);
        if (set == null) return "They have nothing to say.";

        var completed = GetCompletedMissionTitles();
        foreach (PersonDialogueLine line in set.lines)
        {
            if (line.isFallback) continue;
            if (string.IsNullOrEmpty(line.unlockAfterMissionTitle) || completed.Contains(line.unlockAfterMissionTitle))
            {
                return line.dialogueText;
            }
        }

        // Fallback line if nothing matched
        return set.lines.FirstOrDefault(l => l.isFallback)?.dialogueText ?? "…";
    }

    private HashSet<string> GetCompletedMissionTitles()
    {
        MissionSequence sequence = MissionsManager.Instance.MissionSequence;
        int index = MissionsManager.Instance.GetCurrentMissionNumber() - 1;
        return sequence.Missions
            .Take(index)
            .Select(m => m.missionTitle)
            .ToHashSet();
    }
}
