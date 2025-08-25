using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;
using NativeWebSocket;
using System.Text;
using System;
using Newtonsoft.Json;
using Assets.Scripts.ServerIntegration;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.ServerIntegration
{
    public class UniqueKeyManager : Singleton<UniqueKeyManager>
    {
        private ServerCommunicator m_communicator;
        public string gameKey { get; private set; }


        private void Awake()
        {
            gameKey = "";
            m_communicator = new ServerCommunicator(ServerCommunicator.Endpoint.GenerateKey);

        }

        private IEnumerator getUniqueKey()
        {
            UnityWebRequest request = UnityWebRequest.Get(m_communicator.ServerUrl);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string receivedJson = request.downloadHandler.text;
                try
                {
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new OperatorConverter());

                    Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(receivedJson, settings);
                    gameKey = result["key"];
                    Debug.Log($"gameKey value returned from server : {gameKey}");

                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Failed to parse full Query object: {ex.Message}");
                }
            }
            else if (request.responseCode == 204)
            {
                Debug.Log("⏳ Server responded with 204 No Content — no new query yet.");
            }
            else
            {
                Debug.LogError($"❌ Failed to fetch query: {request.responseCode} | {request.error}");
            }


        }

        public void SetGameKeyFromSavedGame(string i_gameKey)
        {
            gameKey = i_gameKey;
        }

        public void GenerateGameKey()
        {
            if (gameKey == null || gameKey.Equals(""))
            {
                StartCoroutine(getUniqueKey());
            }
            Debug.Log($"gameKey : {gameKey}");
        }

        public bool CompareKeys(string i_Input)
        {
            Debug.Log($"i_Input: {i_Input}");
            Debug.Log($"Key: {gameKey}");
            
            return i_Input == gameKey;
        }
    }
}
