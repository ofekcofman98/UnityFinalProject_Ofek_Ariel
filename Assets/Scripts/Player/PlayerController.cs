using Assets.Scripts.ServerIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool sqlMode;
    private bool isMoving;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    void Start()
    {
        sqlMode = false;
        isMoving = true;

        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

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

    public void ResetToStart()
    {
        CharacterController cc = GetComponent<CharacterController>();

        // Disable before moving to avoid physics glitches
        if (cc != null) cc.enabled = false;

        transform.position = _initialPosition;
        transform.rotation = _initialRotation;

        if (cc != null) cc.enabled = true;
    }

}
