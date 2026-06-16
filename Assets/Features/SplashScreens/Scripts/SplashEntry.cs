#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.Video;

namespace JamTemplate.SplashScreens
{
    public enum SplashMediaType
    {
        Image,
        Video,
    }

    /// <summary>
    /// A single splash screen: either a still image or a video clip, shown over a
    /// solid background colour, with an optional sound. Fade in/out is handled by
    /// the scene transition assigned on the SplashScreenController.
    /// </summary>
    [System.Serializable]
    public class SplashEntry
    {
        [Header("Media")]
#if ODIN_INSPECTOR
        [EnumToggleButtons]
        [ValidateInput(nameof(ValidateMedia))]
#endif
        [Tooltip("Show a still Image or play a Video for this splash.")]
        public SplashMediaType mediaType = SplashMediaType.Image;

#if ODIN_INSPECTOR
        [ShowIf(nameof(mediaType), SplashMediaType.Image)]
        [PreviewField(55)]
#endif
        [Tooltip("Sprite shown when Media Type is Image. Kept at its original aspect ratio (letterboxed).")]
        public Sprite image;

#if ODIN_INSPECTOR
        [ShowIf(nameof(mediaType), SplashMediaType.Video)]
#endif
        [Tooltip("Clip played when Media Type is Video. The splash holds until the clip finishes. Does not work on WebGL — set Video Url instead.")]
        public VideoClip video;

#if ODIN_INSPECTOR
        [ShowIf(nameof(mediaType), SplashMediaType.Video)]
#endif
        [Tooltip("Streams the video from a URL, or a path relative to StreamingAssets (e.g. intro.mp4 for Assets/StreamingAssets/intro.mp4). Takes precedence over the clip, and is the only way to play splash video on WebGL.")]
        public string videoUrl;

        [Tooltip("Solid colour filling the screen behind the image or video.")]
        public Color backgroundColor = Color.black;

        [Header("Timing")]
#if ODIN_INSPECTOR
        [ShowIf(nameof(mediaType), SplashMediaType.Image)]
#endif
        [Min(0f)]
        [Tooltip("Seconds the splash stays fully visible. Video holds until the clip ends instead.")]
        public float holdDuration = 1.5f;

        [Header("Audio")]
        [Tooltip("Optional sound played once when the splash appears.")]
        public AudioClip audioClip;

#if ODIN_INSPECTOR
        [ShowIf("@audioClip != null")]
#endif
        [Range(0f, 1f)]
        [Tooltip("Playback volume for the optional sound.")]
        public float audioVolume = 1f;

        /// <summary>Whether this entry has any video source (clip or URL).</summary>
        public bool HasVideoSource => video != null || !string.IsNullOrEmpty(videoUrl);

        /// <summary>
        /// The URL to stream, or null to use the clip. Relative paths resolve
        /// under <see cref="Application.streamingAssetsPath"/>.
        /// </summary>
        public string ResolveVideoUrl()
        {
            if (string.IsNullOrEmpty(videoUrl))
                return null;

            return videoUrl.Contains("://") ? videoUrl : Application.streamingAssetsPath + "/" + videoUrl;
        }

        /// <summary>How to refer to this entry's video in log messages.</summary>
        internal string VideoLabel => !string.IsNullOrEmpty(videoUrl) ? videoUrl : video != null ? video.name : "(none)";

        /// <summary>Label shown for this entry in the controller's Odin list drawer.</summary>
        internal string EditorLabel
        {
            get
            {
                if (mediaType == SplashMediaType.Video)
                    return $"Video: {VideoLabel}";
                return image != null ? $"Image: {image.name}" : "Image: (none)";
            }
        }

        private bool ValidateMedia(SplashMediaType type, ref string errorMessage)
        {
            if (type == SplashMediaType.Image && image == null)
            {
                errorMessage = "Media Type is Image, but no Image sprite is assigned.";
                return false;
            }

            if (type == SplashMediaType.Video && !HasVideoSource)
            {
                errorMessage = "Media Type is Video, but no Video clip or URL is assigned.";
                return false;
            }

            return true;
        }
    }
}
