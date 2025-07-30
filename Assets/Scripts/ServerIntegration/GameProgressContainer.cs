using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.ServerIntegration
{
    public class GameProgressContainer
    {
        public bool SqlMode { get; set; }
        public int currentMissionIndex { get; set; }

        public int Lives { get; set; }
        public GameProgressContainer(bool i_sqlMode, int i_currentMissionIndex, int i_Lives)
        {
            SqlMode = i_sqlMode;
            currentMissionIndex = i_currentMissionIndex;
            Lives = i_Lives;
        }

    }
}
