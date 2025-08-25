using System;
using UnityEngine;

namespace Assets.Scripts.ServerIntegration
{
    public class ScreensaverManager : MonoBehaviour
    {
        // public static event Action OnUserInteraction;
        // public static event Action OnIdleTimeoutReached;

        // public float idleTimeToActivate = 10f;
        // private float lastTouchTime;
        // private Coroutine idleTimerCoroutine;
        // private bool screensaverActive = false;
        private GameObject mobileCanvas;
        private GameObject screensaverCanvas;


        public void Init(GameObject i_mobileCanvas, GameObject i_screensaverCanvas)
        {
            mobileCanvas = i_mobileCanvas;
            screensaverCanvas = i_screensaverCanvas;

            if (!Application.isMobilePlatform)
            {
                enabled = false;
                return;
            }

            // Show screensaver by default
            if (screensaverCanvas != null) screensaverCanvas.SetActive(true);
            if (mobileCanvas != null) mobileCanvas.SetActive(false);

            Debug.Log("📱 Screensaver active by default on mobile.");
        }

        

        // void Start()
        // {
        //     // if (!Application.isMobilePlatform)
        //     // {
        //     //     Debug.Log("🖥 Not mobile — disabling ScreensaverManager.");
        //     //     enabled = false;
        //     //     return;
        //     // }

        //     // Debug.Log("📱 ScreensaverManager started on mobile.");
        //     // OnUserInteraction += HandleUserInteraction;
        //     // OnIdleTimeoutReached += ActivateScreensaver;
        //     // lastTouchTime = Time.realtimeSinceStartup;
        //     if (!Application.isMobilePlatform)
        //     {
        //         enabled = false;
        //         return;
        //     }

        //     // Show screensaver first, hide game UI
        //     if (screensaverCanvas != null) screensaverCanvas.SetActive(true);
        //     if (mobileCanvas != null) mobileCanvas.SetActive(false);

        //     Debug.Log("📱 Screensaver active by default on mobile.");


        // }

        // void Update()
        // {
        //     float now = Time.realtimeSinceStartup;

        //     #if UNITY_EDITOR || UNITY_STANDALONE
        //                 if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
        //                 {
        //                     lastTouchTime = now;
        //                     Debug.Log($"🖱 PC input detected — lastTouchTime = {lastTouchTime}");
        //                     OnUserInteraction?.Invoke();
        //                 }
        //     #else
        //         // Mobile input support
        //         if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        //         {
        //             lastTouchTime = now;
        //             Debug.Log($"📍 Touch — lastTouchTime = {lastTouchTime}");
        //             OnUserInteraction?.Invoke();
        //         }
        //     #endif

        //     float idleDuration = now - lastTouchTime;
        //     Debug.Log($"🕐 Idle duration (real-time): {idleDuration}");

        //     if (idleDuration >= idleTimeToActivate && !screensaverActive)
        //     {
        //         Debug.Log("🔒 Real-time idle triggered.");
        //         OnIdleTimeoutReached?.Invoke();
        //     }
        // }


        // private void HandleUserInteraction()
        // {
        //     Debug.Log($"🌀 HandleUserInteraction called. screensaverActive = {screensaverActive}");

        //     if (screensaverActive)
        //     {
        //         Debug.Log("📴 Deactivating screensaver.");
        //         DeactivateScreensaver();
        //     }

        //     RestartIdleCountdown();
        // }

        // private void StartIdleCountdown()
        // {
        //     if (idleTimerCoroutine != null)
        //         StopCoroutine(idleTimerCoroutine);

        //     idleTimerCoroutine = StartCoroutine(IdleTimeoutCoroutine());
        // }

        // private void RestartIdleCountdown()
        // {
        //     StartIdleCountdown();
        // }

        // private System.Collections.IEnumerator IdleTimeoutCoroutine()
        // {
        //     Debug.Log($"⏳ Starting {idleTimeToActivate}-second idle countdown.");
        //     yield return new WaitForSeconds(idleTimeToActivate);
        //     Debug.Log("🟦 Idle timeout reached — broadcasting OnIdleTimeoutReached");
        //     OnIdleTimeoutReached?.Invoke();
        // }

        // private void ActivateScreensaver()
        // {
        //     var mobileCanvas = GameManager.Instance?.GetType().GetField("mobileCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(GameManager.Instance) as GameObject;
        //     var screensaverCanvas = GameManager.Instance?.GetType().GetField("mobileScreensaverCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(GameManager.Instance) as GameObject;

        //     if (mobileCanvas != null) mobileCanvas.SetActive(false);
        //     if (screensaverCanvas != null) screensaverCanvas.SetActive(true);

        //     screensaverActive = true;
        //     Debug.Log("🌙 Screensaver Activated (mobile)");
        // }

        public void DeactivateScreensaver()
        {
            // var mobileCanvas = GameManager.Instance?.GetType().GetField("mobileCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(GameManager.Instance) as GameObject;
            // var screensaverCanvas = GameManager.Instance?.GetType().GetField("mobileScreensaverCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(GameManager.Instance) as GameObject;

            // if (mobileCanvas != null) mobileCanvas.SetActive(true);
            // if (screensaverCanvas != null) screensaverCanvas.SetActive(false);

            // screensaverActive = false;
            // Debug.Log("📱 Main Canvas Reactivated (mobile)");
            if (screensaverCanvas != null) screensaverCanvas.SetActive(false);
            if (mobileCanvas != null) mobileCanvas.SetActive(true);

            Debug.Log("📱 Screensaver deactivated by SQL Mode button.");

        }

        // private void OnDestroy()
        // {
        //     OnUserInteraction -= HandleUserInteraction;
        //     OnIdleTimeoutReached -= ActivateScreensaver;
        // }
    }
}
