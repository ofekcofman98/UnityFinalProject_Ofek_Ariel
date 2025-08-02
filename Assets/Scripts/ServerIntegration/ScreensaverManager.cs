using System;
using UnityEngine;

namespace Assets.Scripts.ServerIntegration
{
    public class ScreensaverManager : MonoBehaviour
    {
        public static event Action OnUserInteraction;
        public static event Action OnIdleTimeoutReached;

        public float idleTimeToActivate = 10f;
        private float lastTouchTime;
        private Coroutine idleTimerCoroutine;
        private bool screensaverActive = false;

        void Start()
        {
            if (!Application.isMobilePlatform)
            {
                Debug.Log("🖥 Not mobile — disabling ScreensaverManager.");
                enabled = false;
                return;
            }

            Debug.Log("📱 ScreensaverManager started on mobile.");
            OnUserInteraction += HandleUserInteraction;
            OnIdleTimeoutReached += ActivateScreensaver;
            lastTouchTime = Time.realtimeSinceStartup;

            //StartIdleCountdown();
        }

        //void Update()
        //{
        //    if (Input.touchCount > 0)
        //    {
        //        Touch touch = Input.GetTouch(0);
        //        if (touch.phase == TouchPhase.Began)
        //        {
        //            Debug.Log("👆 TouchPhase.Began detected — broadcasting OnUserInteraction");
        //            OnUserInteraction?.Invoke();
        //        }
        //    }
        //}
        void Update()
        {
            float now = Time.realtimeSinceStartup;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                lastTouchTime = now;
                Debug.Log($"📍 Touch — lastTouchTime = {lastTouchTime}");
            }

            float idleDuration = now - lastTouchTime;
            Debug.Log($"🕐 Idle duration (real-time): {idleDuration}");

            if (idleDuration >= 10f)
            {
                Debug.Log("🔒 Real-time idle triggered.");
                OnIdleTimeoutReached?.Invoke();
            }
        }

        private void HandleUserInteraction()
        {
            Debug.Log($"🌀 HandleUserInteraction called. screensaverActive = {screensaverActive}");

            if (screensaverActive)
            {
                Debug.Log("📴 Deactivating screensaver.");
                DeactivateScreensaver();
            }

            RestartIdleCountdown();
        }

        private void StartIdleCountdown()
        {
            if (idleTimerCoroutine != null)
                StopCoroutine(idleTimerCoroutine);

            idleTimerCoroutine = StartCoroutine(IdleTimeoutCoroutine());
        }

        private void RestartIdleCountdown()
        {
            StartIdleCountdown();
        }

        private System.Collections.IEnumerator IdleTimeoutCoroutine()
        {
            Debug.Log($"⏳ Starting {idleTimeToActivate}-second idle countdown.");
            yield return new WaitForSeconds(idleTimeToActivate);
            Debug.Log("🟦 Idle timeout reached — broadcasting OnIdleTimeoutReached");
            OnIdleTimeoutReached?.Invoke();
        }

        private void ActivateScreensaver()
        {
            var mobileCanvas = GameManager.Instance?.GetType().GetField("mobileCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(GameManager.Instance) as GameObject;
            var screensaverCanvas = GameManager.Instance?.GetType().GetField("mobileScreensaverCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(GameManager.Instance) as GameObject;

            if (mobileCanvas != null) mobileCanvas.SetActive(false);
            if (screensaverCanvas != null) screensaverCanvas.SetActive(true);
            //GameManager.Instance.SwitchMobileCanvas(false);

            screensaverActive = true;
            Debug.Log("🌙 Screensaver Activated (mobile)");
        }

        private void DeactivateScreensaver()
        {
            var mobileCanvas = GameManager.Instance?.GetType().GetField("mobileCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(GameManager.Instance) as GameObject;
            var screensaverCanvas = GameManager.Instance?.GetType().GetField("mobileScreensaverCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(GameManager.Instance) as GameObject;

            if (mobileCanvas != null) mobileCanvas.SetActive(true);
            if (screensaverCanvas != null) screensaverCanvas.SetActive(false);
            //GameManager.Instance.SwitchMobileCanvas(true);

            screensaverActive = false;
            Debug.Log("📱 Main Canvas Reactivated (mobile)");
        }

        private void OnDestroy()
        {
            OnUserInteraction -= HandleUserInteraction;
            OnIdleTimeoutReached -= ActivateScreensaver;
        }
    }
}
