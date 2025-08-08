using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSfx : MonoBehaviour
{
    [SerializeField] private AudioCue clickCue;

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (clickCue != null)
            {
                SfxManager.Instance.Play2D(clickCue);
            }
        });
    }
}
