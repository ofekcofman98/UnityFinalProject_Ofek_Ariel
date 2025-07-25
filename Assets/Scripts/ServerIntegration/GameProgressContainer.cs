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
        [SerializeField] public bool SqlMode { get; set; }
        [SerializeField] public MissionsManager missionManager;

        public GameProgressContainer(bool sqlMode, MissionsManager missionManager)
        {
            SqlMode = sqlMode;
            this.missionManager = missionManager;
        }

    }
}
