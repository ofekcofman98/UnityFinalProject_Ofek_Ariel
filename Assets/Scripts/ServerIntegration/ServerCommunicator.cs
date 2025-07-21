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
        private const string k_pcIP = ServerData.k_pcIP;
        private string serverUrl;
        private bool m_isMobile = Application.isMobilePlatform;
        private bool _isRunning = false;
        private string m_resource;

        public ServerCommunicator(string i_resource)
        {
            m_resource = i_resource;
            serverUrl = k_pcIP + i_resource;
        }

        protected string ServerUrl => serverUrl;
        protected string Resource => m_resource;
        protected bool IsRunning => _isRunning;
        protected bool IsMobile => m_isMobile;
    }
}
