using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Renders a camera into a texture, blurs it, and shows the result as a
    /// full-screen backdrop — a "blurred scene" background for menus.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Scene Blur Background")]
    public class SceneBlurBackground : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Camera to capture and blur. Defaults to the main camera.")]
        private Camera sourceCamera;

        [SerializeField]
        [Range(1, 4)]
        [Tooltip("Resolution divisor for the blur buffers — higher is cheaper and softer.")]
        private int downsample = 2;

        [SerializeField]
        [Range(1, 8)]
        [Tooltip("Number of blur passes — higher is blurrier.")]
        private int iterations = 4;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Blur kernel spread.")]
        private float blurSize = 1.5f;

        [SerializeField]
        [Tooltip("Sort order of the backdrop canvas — keep below your menu canvas.")]
        private int sortingOrder = -100;

        private Camera cam;
        private Material blurMaterial;
        private RenderTexture sceneTexture;
        private RenderTexture resultTexture;
        private RawImage display;
        private Coroutine blurRoutine;

        private void OnEnable()
        {
            cam = sourceCamera != null ? sourceCamera : Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[SceneBlur] No camera available to blur.", this);
                enabled = false;
                return;
            }

            Shader shader = Shader.Find("Hidden/JamTemplate/Blur");
            if (shader == null)
            {
                Debug.LogWarning("[SceneBlur] Blur shader not found.", this);
                enabled = false;
                return;
            }

            blurMaterial = new Material(shader);
            BuildDisplay();
            CreateSceneTexture();
            blurRoutine = StartCoroutine(BlurLoop());
        }

        private void OnDisable()
        {
            if (blurRoutine != null)
                StopCoroutine(blurRoutine);
            if (cam != null && cam.targetTexture == sceneTexture)
                cam.targetTexture = null;

            Release(ref sceneTexture);
            Release(ref resultTexture);
            if (blurMaterial != null)
                Destroy(blurMaterial);
        }

        private void BuildDisplay()
        {
            int uiLayer = LayerMask.NameToLayer("UI");

            var canvasObject = new GameObject("Scene Blur Canvas", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.layer = uiLayer;
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var imageObject = new GameObject("Blurred Scene", typeof(RawImage));
            imageObject.layer = uiLayer;
            var rect = (RectTransform)imageObject.transform;
            rect.SetParent(canvasObject.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            display = imageObject.GetComponent<RawImage>();
            display.raycastTarget = false;
        }

        private void CreateSceneTexture()
        {
            sceneTexture = new RenderTexture(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height), 16);
            sceneTexture.Create();
            cam.targetTexture = sceneTexture;
        }

        private IEnumerator BlurLoop()
        {
            var endOfFrame = new WaitForEndOfFrame();
            while (true)
            {
                yield return endOfFrame;

                if (sceneTexture == null ||
                    sceneTexture.width != Screen.width || sceneTexture.height != Screen.height)
                {
                    if (cam != null && cam.targetTexture == sceneTexture)
                        cam.targetTexture = null;
                    Release(ref sceneTexture);
                    CreateSceneTexture();
                    continue;
                }

                Blur();
            }
        }

        private void Blur()
        {
            int width = Mathf.Max(1, sceneTexture.width / downsample);
            int height = Mathf.Max(1, sceneTexture.height / downsample);

            if (resultTexture == null || resultTexture.width != width || resultTexture.height != height)
            {
                Release(ref resultTexture);
                resultTexture = new RenderTexture(width, height, 0);
                resultTexture.Create();
                display.texture = resultTexture;
            }

            RenderTexture temp = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(sceneTexture, resultTexture);
            blurMaterial.SetFloat("_BlurSize", blurSize);
            for (int i = 0; i < iterations; i++)
            {
                Graphics.Blit(resultTexture, temp, blurMaterial, 0);
                Graphics.Blit(temp, resultTexture, blurMaterial, 1);
            }

            RenderTexture.ReleaseTemporary(temp);
        }

        private static void Release(ref RenderTexture texture)
        {
            if (texture == null)
                return;
            texture.Release();
            Destroy(texture);
            texture = null;
        }
    }
}
