using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreensaverController
{
    private GameObject mobileCanvas;
    private GameObject screensaverCanvas;

    public ScreensaverController(GameObject i_mobileCanvas, GameObject i_screensaverCanvas)
    {
        mobileCanvas = i_mobileCanvas;
        screensaverCanvas = i_screensaverCanvas;
    }


    public void ShowScreensaver()
    {
        if (screensaverCanvas == null)
        {
            Debug.LogError("❌ screensaverCanvas is NULL!");
        }

        if (mobileCanvas == null)
        {
            Debug.LogError("❌ mobileCanvas is NULL!");
        }

        screensaverCanvas?.SetActive(true);
        mobileCanvas?.SetActive(false);

        Debug.Log("📱 Screensaver shown by default.");
    }

    public void HideScreensaver()
    {
        screensaverCanvas?.SetActive(false);
        mobileCanvas?.SetActive(true);
        Debug.Log("📱 Screensaver hidden by SQL button.");
    }
}
