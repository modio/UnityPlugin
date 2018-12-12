using System;
using MonoBehaviour = UnityEngine.MonoBehaviour;
using Texture2D = UnityEngine.Texture2D;

namespace ModIO.UI
{
    [System.Serializable]
    public struct ImageDisplayData
    {
        public enum MediaType
        {
            ModLogo,
            ModGalleryImage,
            ModYouTubeThumbnail,
        };

        public int modId;
        public MediaType mediaType;
        public string imageId;
        public Texture2D texture;

        public string fileName  { get { return imageId; } set { imageId = value; } }
        public string youTubeId { get { return imageId; } set { imageId = value; } }
    }

    public interface IImageDataDisplay
    {
        ImageDisplayData data { get; set; }
    }

    public abstract class ModMediaDisplayComponent : MonoBehaviour, IImageDataDisplay
    {
        public abstract event Action<ModMediaDisplayComponent> logoClicked;
        public abstract event Action<ModMediaDisplayComponent> galleryImageClicked;
        public abstract event Action<ModMediaDisplayComponent> youTubeThumbnailClicked;

        public abstract LogoSize logoSize                       { get; }
        public abstract ModGalleryImageSize galleryImageSize    { get; }

        public abstract ImageDisplayData data           { get; set; }

        public abstract void Initialize();
        public abstract void DisplayLogo(int modId, LogoImageLocator locator);
        public abstract void DisplayGalleryImage(int modId, GalleryImageLocator locator);
        public abstract void DisplayYouTubeThumbnail(int modId, string youTubeVideoId);
        public abstract void DisplayLoading();
    }

    public abstract class ModLogoDisplayComponent : MonoBehaviour, IImageDataDisplay
    {
        public abstract event System.Action<ModLogoDisplayComponent> onClick;

        public abstract LogoSize logoSize       { get; }
        public abstract ImageDisplayData data   { get; set; }

        public abstract void Initialize();
        public abstract void DisplayLogo(int modId, LogoImageLocator locator);
        public abstract void DisplayLoading();
    }

    public abstract class ModGalleryImageDisplayComponent : MonoBehaviour, IImageDataDisplay
    {
        public abstract event System.Action<ModGalleryImageDisplayComponent> onClick;

        public abstract ModGalleryImageSize imageSize   { get; }
        public abstract ImageDisplayData data           { get; set; }

        public abstract void Initialize();
        public abstract void DisplayImage(int modId, GalleryImageLocator locator);
        public abstract void DisplayLoading();
    }

    public abstract class YouTubeThumbnailDisplayComponent : MonoBehaviour, IImageDataDisplay
    {
        public abstract event System.Action<YouTubeThumbnailDisplayComponent> onClick;

        public abstract ImageDisplayData data           { get; set; }

        public abstract void Initialize();
        public abstract void DisplayThumbnail(int modId, string youTubeVideoId);
        public abstract void DisplayLoading();
    }
}
