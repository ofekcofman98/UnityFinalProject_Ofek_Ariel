using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CanvasSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject pcCanvas;
    [SerializeField] private GameObject mobileCanvas;
    enum Platform 
    {
         PC,
        Mobile
    }
    void Start()
    {
#if UNITY_EDITOR
        bool isMobileSimulated = false;
        SetActiveCanvas(isMobileSimulated ? Platform.Mobile : Platform.PC);
#else
        if (Application.isMobilePlatform)
        {
            SetActiveCanvas(Platform.Mobile);
        }
        else
        {
            SetActiveCanvas(Platform.PC);
        }
#endif
    }
    private void SetActiveCanvas(Platform platform)
    {
        pcCanvas.SetActive(platform == Platform.PC);
        mobileCanvas.SetActive(platform == Platform.Mobile);
    }

    public void ShowSqlModeCanvas(bool isSqlMode)
    {
        pcCanvas.SetActive(!isSqlMode);
        mobileCanvas.SetActive(isSqlMode);
    }

}
