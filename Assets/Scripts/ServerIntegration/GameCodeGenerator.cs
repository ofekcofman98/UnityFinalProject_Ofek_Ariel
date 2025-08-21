using UnityEngine;
using UnityEngine.UI;



namespace Assets.Scripts.ServerIntegration
{
    public class GameCodeGenerator : Singleton<GameCodeGenerator>
    {
        public string SessionId = "";


        private void Awake()
        {

        }

        public string GetSessionId()
        {
            return SessionId;
        }

        public void ObtainSessionIdFromServer()
        {
            StartCoroutine(GameProgressSender.Instance.getUniqueKey());
        }
    }
}
