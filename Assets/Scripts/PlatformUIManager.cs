using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using UnityEngine;

public class PlatformUIManager : MonoBehaviour
{

    [Header("Canvases")]
    [SerializeField] private GameObject pcGameCanvas;
    [SerializeField] private GameObject pcQueryCanvas;
    [SerializeField] private GameObject mobileCanvas;

    [Header("3D")]
    [SerializeField] private GameObject worldRoot;


    

    void Start()
    {
        bool isMobile = Application.isMobilePlatform;

        if (pcGameCanvas != null) pcGameCanvas.SetActive(!isMobile);
        if (pcQueryCanvas != null) pcQueryCanvas.SetActive(!isMobile);
        if (mobileCanvas != null) mobileCanvas.SetActive(isMobile);

        if (isMobile)
        {
            DisableWorldInteraction();

            if (worldRoot != null) worldRoot.SetActive(false);
            //InitMobile();
        }
        else
        {
            Debug.Log("🖥 Running on PC — activating PC UI");
        }

    }

    //public void InitMobile()
    //{
    //    Debug.Log("🖥 Running on Mobile — inside the InitMobile method");
    //    StateListener.Instance.StartListening();
    //    Debug.Log("🖥 Running on Mobile — after StateListener listening and before ResetListener listening");
    //    ResetListener.Instance.StartListening();
    //    Debug.Log("🖥 Running on Mobile — after ResetListener listening and before ResetGame");
    //    GameManager.Instance.ResetGame();
    //}

    private void DisableWorldInteraction()
    {
        var movement = FindObjectOfType<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        // Disable mouse look / camera rotation
        var mouseLook = FindObjectOfType<MouseLook>();
        if (mouseLook != null)
            mouseLook.enabled = false;

        // // Optional: disable player controller input (e.g., spacebar toggle)
        // var controller = FindObjectOfType<PlayerController>();
        // if (controller != null)
        //     controller.enabled = false;

        // Optional: disable character controller completely
        var characterController = FindObjectOfType<CharacterController>();
        if (characterController != null)
            characterController.enabled = false;

    }
}
