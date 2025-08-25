using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreensaverController
{
    [SerializeField] private Button ConnectButton;
    [SerializeField] private Button BuildSQLButton;
    [SerializeField] private TMP_InputField loadGameInput;
    [SerializeField] private TextMeshProUGUI Comment; // says if load succeed ot not 

    private GameObject mobileCanvas;
    private GameObject screensaverCanvas;

    public ScreensaverController(GameObject i_mobileCanvas, GameObject i_screensaverCanvas)
    {
        mobileCanvas = i_mobileCanvas;
        screensaverCanvas = i_screensaverCanvas;
    }


    public void OnConnectClicked()
    {

    }

    public void OnStartClicked()
    {

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
