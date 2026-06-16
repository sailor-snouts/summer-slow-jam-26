using System.Collections;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace JamTemplate.SplashScreens
{
    /// <summary>
    /// Plays a list of splash screens at game start, then loads the configured next
    /// scene. Builds its own full-screen canvas at runtime, so it can be dropped into
    /// an otherwise empty scene.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Splash Screen Controller")]
    [DisallowMultipleComponent]
    public class SplashScreenController : MonoBehaviour
    {
        [Header("Next Scene")]
        [SerializeField]
#if ODIN_INSPECTOR
        [ValueDropdown(nameof(GetSceneNames))]
        [ValidateInput(nameof(ValidateNextScene))]
#endif
        [Tooltip("Scene loaded after the last splash. Picked from Build Settings.")]
        private string nextSceneName = "SampleScene";

        [Header("Splash Sequence")]
        [SerializeField]
#if ODIN_INSPECTOR
        [ListDrawerSettings(ListElementLabelName = nameof(SplashEntry.EditorLabel))]
#endif
        [Tooltip("The splash screens to play, in order.")]
        private List<SplashEntry> entries = new List<SplashEntry>();

        [Header("Skip")]
        [SerializeField]
        [Tooltip("Let the player skip with any key/click/tap/gamepad button. Escape skips the whole sequence.")]
        private bool allowSkip = true;

        [SerializeField]
#if ODIN_INSPECTOR
        [ShowIf(nameof(allowSkip))]
        [Indent]
#endif
        [Min(0f)]
        [Tooltip("Skip input is ignored for this long so a button held from a previous scene can't blow past the first splash.")]
        private float skipInputDelay = 0.35f;

        private enum SkipKind { None, Current, All }

        private Image background;
        private Image imageDisplay;
        private RawImage videoDisplay;
        private AspectRatioFitter videoAspectFitter;
        private AudioSource audioSource;
        private VideoPlayer videoPlayer;
        private RenderTexture videoRenderTexture;

        private float startTime;
        private bool skipRequested;
        private bool skipAllRequested;
        private bool videoFinished;
        private bool videoFailed;

        private void Awake()
        {
            startTime = Time.unscaledTime;
            BuildUI();
        }

        private void Start()
        {
            StartCoroutine(RunSequence());
        }

        private void Update()
        {
            if (!allowSkip || skipAllRequested)
                return;
            if (Time.unscaledTime - startTime < skipInputDelay)
                return;

            switch (DetectInput())
            {
                case SkipKind.Current:
                    skipRequested = true;
                    break;
                case SkipKind.All:
                    skipAllRequested = true;
                    break;
            }
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoLoopPointReached;
                videoPlayer.errorReceived -= OnVideoError;
            }

            if (videoRenderTexture != null)
            {
                videoRenderTexture.Release();
                Destroy(videoRenderTexture);
                videoRenderTexture = null;
            }
        }

        private void BuildUI()
        {
            int uiLayer = LayerMask.NameToLayer("UI");

            var canvasObject = new GameObject("Splash Canvas",
                typeof(Canvas), typeof(CanvasScaler));
            canvasObject.layer = uiLayer;
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Below the scene transition overlay (short.MaxValue) so it covers this.
            canvas.sortingOrder = 1000;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            background = CreateStretchChild<Image>("Background", canvasObject.transform, uiLayer);
            background.color = Color.black;

            imageDisplay = CreateStretchChild<Image>("Image", canvasObject.transform, uiLayer);
            imageDisplay.preserveAspect = true;
            imageDisplay.enabled = false;

            videoDisplay = CreateStretchChild<RawImage>("Video", canvasObject.transform, uiLayer);
            videoDisplay.enabled = false;
            videoAspectFitter = videoDisplay.gameObject.AddComponent<AspectRatioFitter>();
            videoAspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            // Splash videos play visuals only; use a SplashEntry's Audio Clip field
            // for sound. Disabling audio output stops Unity decoding the video's
            // audio track, which is what triggered the AudioSampleProvider overflow.
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            // Never skip frames — a splash video is short and we want every frame
            // shown. skipOnDrop:true makes the player jump ahead after a hitch
            // (e.g. the editor entering Play mode), skipping straight to the end.
            videoPlayer.skipOnDrop = false;
            videoPlayer.loopPointReached += OnVideoLoopPointReached;
            videoPlayer.errorReceived += OnVideoError;
        }

        private static T CreateStretchChild<T>(string name, Transform parent, int layer) where T : Graphic
        {
            var child = new GameObject(name, typeof(T));
            child.layer = layer;

            var rect = child.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var graphic = child.GetComponent<T>();
            graphic.raycastTarget = false;
            return graphic;
        }

        private void EnsureVideoRenderTexture(int width, int height)
        {
            if (width <= 0)
                width = 1920;
            if (height <= 0)
                height = 1080;

            if (videoRenderTexture != null &&
                (videoRenderTexture.width != width || videoRenderTexture.height != height))
            {
                videoRenderTexture.Release();
                Destroy(videoRenderTexture);
                videoRenderTexture = null;
            }

            if (videoRenderTexture == null)
            {
                videoRenderTexture = new RenderTexture(width, height, 0);
                videoRenderTexture.Create();
            }
        }

        private IEnumerator RunSequence()
        {
            // Start covered so the first splash fades in.
            SplashTransition.SnapCovered();

            foreach (var entry in entries)
            {
                if (skipAllRequested)
                    break;
                if (entry == null)
                    continue;
                yield return PlayEntry(entry);
            }

            LoadNextScene();
        }

        private IEnumerator PlayEntry(SplashEntry entry)
        {
            skipRequested = false;
            background.color = entry.backgroundColor;

            bool isVideo = entry.mediaType == SplashMediaType.Video && entry.HasVideoSource;

            // Prepare the entry's content while the screen is still covered.
            if (isVideo)
            {
                string url = entry.ResolveVideoUrl();
#if UNITY_WEBGL && !UNITY_EDITOR
                // Imported VideoClips can't play on WebGL; only URL streaming works.
                if (string.IsNullOrEmpty(url))
                {
                    Debug.LogWarning($"[SplashScreen] Video '{entry.VideoLabel}' can't play on WebGL — set the entry's Video Url (StreamingAssets) instead of a clip. Skipping this splash.", this);
                    yield break;
                }
#endif
                imageDisplay.enabled = false;
                videoFinished = false;
                videoFailed = false;
                if (!string.IsNullOrEmpty(url))
                {
                    videoPlayer.source = VideoSource.Url;
                    videoPlayer.url = url;
                }
                else
                {
                    videoPlayer.source = VideoSource.VideoClip;
                    videoPlayer.clip = entry.video;
                }
                videoPlayer.Prepare();

                float prepareDeadline = Time.unscaledTime + 8f;
                while (!videoPlayer.isPrepared)
                {
                    if (videoFailed || Time.unscaledTime > prepareDeadline)
                    {
                        Debug.LogWarning($"[SplashScreen] Video '{entry.VideoLabel}' could not be prepared; skipping this splash.", this);
                        yield break;
                    }
                    if (ShouldSkip())
                        yield break;
                    yield return null;
                }

                EnsureVideoRenderTexture((int)videoPlayer.width, (int)videoPlayer.height);
                videoPlayer.targetTexture = videoRenderTexture;
                videoDisplay.texture = videoRenderTexture;
                if (videoPlayer.height > 0)
                    videoAspectFitter.aspectRatio = (float)videoPlayer.width / videoPlayer.height;
                videoDisplay.enabled = true;
            }
            else
            {
                videoDisplay.enabled = false;
                imageDisplay.sprite = entry.image;
                imageDisplay.enabled = entry.image != null;
            }

            // Start media and audio, then fade in over them.
            if (isVideo)
                videoPlayer.Play();
            if (entry.audioClip != null)
            {
                audioSource.clip = entry.audioClip;
                audioSource.volume = entry.audioVolume;
                audioSource.Play();
            }

            IEnumerator reveal = SplashTransition.Reveal();
            if (reveal != null)
                yield return reveal;

            // Hold.
            if (isVideo)
            {
                while (!videoFinished && !videoFailed)
                {
                    if (ShouldSkip())
                        break;
                    yield return null;
                }
            }
            else
            {
                float elapsed = 0f;
                while (elapsed < entry.holdDuration)
                {
                    if (ShouldSkip())
                        break;
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            // Fade out.
            IEnumerator cover = SplashTransition.Cover();
            if (cover != null)
                yield return cover;

            if (isVideo)
            {
                videoPlayer.Stop();
                videoPlayer.targetTexture = null;
                videoDisplay.texture = null;
            }
            audioSource.Stop();
            audioSource.clip = null;
        }

        private bool ShouldSkip()
        {
            return skipRequested || skipAllRequested;
        }

        private void LoadNextScene()
        {
            if (string.IsNullOrWhiteSpace(nextSceneName))
            {
                Debug.LogWarning("[SplashScreen] No next scene set; staying on the splash scene.", this);
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                Debug.LogError($"[SplashScreen] Scene '{nextSceneName}' is not in Build Settings. Add it under File > Build Profiles.", this);
                return;
            }

            // Loads behind a transition when Core wired one up, raw otherwise.
            SplashTransition.LoadScene(nextSceneName);
        }

        private bool ValidateNextScene(string sceneName, ref string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                errorMessage = "Set the scene to load after the splash sequence.";
                return false;
            }
#if UNITY_EDITOR
            // Application.CanStreamedLevelBeLoaded is a runtime API and is unreliable
            // in edit mode, so validate against the editor Build Settings list directly.
            foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
            {
                if (scene.enabled &&
                    (scene.path == sceneName ||
                     System.IO.Path.GetFileNameWithoutExtension(scene.path) == sceneName))
                {
                    return true;
                }
            }

            errorMessage = $"'{sceneName}' is not an enabled scene in Build Settings.";
            return false;
#else
            return true;
#endif
        }

        private void OnVideoLoopPointReached(VideoPlayer source)
        {
            videoFinished = true;
        }

        private void OnVideoError(VideoPlayer source, string message)
        {
            videoFailed = true;
            Debug.LogWarning($"[SplashScreen] Video error: {message}", this);
        }

#if ODIN_INSPECTOR
        private static IEnumerable<ValueDropdownItem<string>> GetSceneNames()
        {
            yield return new ValueDropdownItem<string>("(None)", string.Empty);
#if UNITY_EDITOR
            foreach (var buildScene in UnityEditor.EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled)
                    continue;
                string name = System.IO.Path.GetFileNameWithoutExtension(buildScene.path);
                yield return new ValueDropdownItem<string>(name, name);
            }
#endif
        }
#endif

        private static SkipKind DetectInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                    return SkipKind.All;
                if (keyboard.anyKey.wasPressedThisFrame)
                    return SkipKind.Current;
            }

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                return SkipKind.Current;

            var gamepad = Gamepad.current;
            if (gamepad != null && (gamepad.buttonSouth.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame))
                return SkipKind.Current;

            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
                return SkipKind.Current;

            return SkipKind.None;
#else
            if (Input.GetKeyDown(KeyCode.Escape))
                return SkipKind.All;
            if (Input.anyKeyDown)
                return SkipKind.Current;
            return SkipKind.None;
#endif
        }
    }
}
