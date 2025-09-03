using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using UnityEngine;

public class GameSaver : Singleton<GameSaver>
{
    public void SaveGame(Action<string> onGameKeyAvailable = null)
    {
        int lastValidMissionIndex = MissionsManager.Instance.GetLastValidMissionIndex();
        int sequenceIndex = SequenceManager.Instance.CurrentSequenceIndex;
        int lives = LivesManager.Instance.Lives;

        GameProgressContainer gpc = new GameProgressContainer(sequenceIndex, lastValidMissionIndex, lives);

        if (GameProgressSender.Instance.IsSameAsLastSave(gpc))
        {
            Debug.Log("🛑 No changes since last save. Skipping.");
            onGameKeyAvailable?.Invoke(UniqueKeyManager.Instance.gameKey);
            return;
        }

        GameProgressSender.Instance.StartCoroutine(SendAndNotify(gpc, onGameKeyAvailable));
    }

    public IEnumerator SendAndNotify(GameProgressContainer gpc, Action<string> onGameKeyAvailable)
    {
        yield return GameProgressSender.Instance.SendGameProgressToServer(gpc);
        onGameKeyAvailable?.Invoke(UniqueKeyManager.Instance.gameKey);
    }

}
