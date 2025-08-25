using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Assets.Scripts.ServerIntegration
{
    public class ServerCommunicator
    {
        public enum Endpoint
        {
            GetQuery,
            SendQuery,
            GetReset,
            SendReset,
            GetState,         
            SendState,
            GetSQLMode,
            SendSQLMode,
            SendGameProgress,
            GetGameProgress,
            Retrieve,
            Store,
            Echo,
            GenerateKey,
            AllKeys,
            ServerReset,
            FullServerReset,
        }


        private const string k_pcIP = ServerData.k_pcIP;
        private string serverUrl;
        private bool m_isMobile = Application.isMobilePlatform;
        public bool m_isRunning { get; set; } = false;
        private string m_resource;
        public string m_queryParameters { get; private set; }
        public int pollRateMilliSeconds { get; } = 500;

        public int screensaverDelayAfterQuery = 2000;
        public string sessionID { get; set; }

        public ServerCommunicator(Endpoint endpoint)
        {
            m_resource = GetPathForEndpoint(endpoint);
            serverUrl = "https://" + k_pcIP + m_resource;
            addGameKeyAsQueryParams();
        }

        public string ServerUrl => serverUrl;
        public string Resource => m_resource;
        public bool IsMobile => m_isMobile;

        public void addGameKeyAsQueryParams()
        {
            serverUrl += $"?key={UniqueKeyManager.Instance.gameKey}";
        }
        private string GetPathForEndpoint(Endpoint endpoint)
        {
            return endpoint switch
            {
                Endpoint.GetQuery => "/get-query",
                Endpoint.SendQuery => "/send-query",
                Endpoint.GetReset => "/get-reset",
                Endpoint.SendReset => "/send-reset",
                Endpoint.GetState => "/get-state",
                Endpoint.SendState => "/send-state",
                Endpoint.GetSQLMode => "/get-sqlmode",
                Endpoint.SendSQLMode => "/send-sqlmode",      
                Endpoint.SendGameProgress => "/send-gameprogress",
                Endpoint.GetGameProgress => "/get-gameprogress",
                Endpoint.Retrieve => "/retrieve",
                Endpoint.Store => "/store",
                Endpoint.Echo => "/echo",
                Endpoint.GenerateKey => "/generate-key",
                Endpoint.AllKeys => "/all-keys",
                Endpoint.ServerReset => "/server-reset",
                Endpoint.FullServerReset => "/full-server-reset",
                    _ => throw new ArgumentOutOfRangeException(nameof(endpoint), $"Unsupported endpoint: {endpoint}")
            };
        }

    }
}
