using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceManager : MonoBehaviour
{
    public GameObject mobileCanvas;
    public GameObject pcCanvas;

    void Start()
    {
        if (IsMobile())
        {
            mobileCanvas.SetActive(true);
            pcCanvas.SetActive(false);
        }
        else
        {
            mobileCanvas.SetActive(false);
            pcCanvas.SetActive(true);
        }
    }

    bool IsMobile()
    {
        return Application.isMobilePlatform;
    }
}
