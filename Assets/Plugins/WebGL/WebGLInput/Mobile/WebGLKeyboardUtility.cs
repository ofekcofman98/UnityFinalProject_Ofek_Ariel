#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

using UnityEngine;

public static class WebGLKeyboardUtility
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void CloseKeyboard();

    public static void CloseMobileKeyboard()
    {
        try
        {
            CloseKeyboard();   // Calls JS function in WebGL template
        }
        catch
        {
            Debug.LogWarning("⚠️ CloseKeyboard not found in WebGL build.");
        }
    }
#else
    public static void CloseMobileKeyboard() { }
#endif
}
