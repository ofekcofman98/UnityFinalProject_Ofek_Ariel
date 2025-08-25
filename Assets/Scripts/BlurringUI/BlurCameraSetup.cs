// // BlurCameraSetup.cs
// // Attach this to an empty GameObject in your scene to auto-create and configure the Blur Camera
// using UnityEngine;

// [ExecuteInEditMode]
// public class BlurCameraSetup : MonoBehaviour
// {
//     public Camera mainCamera;
//     public RenderTexture blurTexture;
//     public Material blurMaterial;

//     private Camera blurCamera;

//     void Start()
//     {
//         if (!mainCamera) mainCamera = Camera.main;
//         if (!blurTexture)
//         {
//             blurTexture = new RenderTexture(Screen.width / 2, Screen.height / 2, 16);
//             blurTexture.name = "BlurredScene";
//         }

//         SetupBlurCamera();
//         SetupBlurMaterial();
//     }

//     void SetupBlurCamera()
//     {
//         if (blurCamera == null)
//         {
//             GameObject camObj = new GameObject("BlurCamera");
//             blurCamera = camObj.AddComponent<Camera>();
//         }

//         blurCamera.CopyFrom(mainCamera);
//         blurCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);

//         blurCamera.targetTexture = blurTexture;
//         blurCamera.depth = mainCamera.depth - 1;
//         blurCamera.clearFlags = CameraClearFlags.SolidColor;
//         blurCamera.backgroundColor = new Color(0, 0, 0, 0);

//         // Exclude UI and popups
//         blurCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));

//         blurCamera.gameObject.hideFlags = HideFlags.DontSave;
//     }

//     void SetupBlurMaterial()
//     {
//         if (blurMaterial)
//         {
//             blurMaterial.SetFloat("_BlurSize", 1.5f); // tweak blur here
//         }
//     }
// }
