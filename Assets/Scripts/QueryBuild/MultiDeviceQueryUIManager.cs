using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDeviceQueryUIManager : MonoBehaviour
{
    [SerializeField] private QueryUIManager _pcUI;
    [SerializeField] private QueryUIManager _mobileUI;

    public void ShowUI()
    {
        _pcUI.ShowUI();
        _mobileUI.ShowUI();
    }

    public void ShowResult(bool isCorrect)
    {
        _pcUI.ShowResult(isCorrect);
        _mobileUI.ShowResult(isCorrect);
    }
}
