using Assets.Scripts.ServerIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool sqlMode;
    private bool isMoving;
    void Start()
    {
        sqlMode = false;
        isMoving = true;
    }

    // Update is called once per frame
    void Update()
    {
        getInput();
    }

    private void getInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameManager.Instance.SqlMode)
            {
                GameManager.Instance.SetSqlMode();
                //SQLmodeSender.Instance.SendSQLmodeToPhone();
            }
        }
    }
}
