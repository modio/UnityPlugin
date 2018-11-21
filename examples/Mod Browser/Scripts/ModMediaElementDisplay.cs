using System;
using UnityEngine;
using UnityEngine.UI;
using ModIO;

public class ModMediaElementDisplay : MonoBehaviour
{
    // ---------[ FIELDS ]---------
    // TODO(@jackson): Add click events

    [Header("Settings")]
    public LogoSize logoSize;
    public ModGalleryImageSize galleryImageSize;

    [Header("UI Components")]
    public Image image;
    public GameObject loadingPlaceholder;
    public GameObject logoOverlay;
    public GameObject youTubeOverlay;
    public GameObject galleryImageOverlay;

    [Header("Display Data")]
    [SerializeField] private int m_modId = -1;
    [SerializeField] private string m_mediaId = string.Empty;

    // ---------[ INITIALIZATION ]---------
    public void Initialize()
    {
        Debug.Assert(image != null);
    }

    // ---------[ UI FUNCTIONALITY ]---------
    public void DisplayModLogo(int modId, LogoImageLocator logoLocator)
    {
        Debug.Assert(modId > 0,
                     "[mod.io] Mod Id needs to be set to a valid mod profile id.");
        Debug.Assert(logoLocator != null,
                     "[mod.io] logoLocator needs to be set and have a fileName.");

        m_modId = modId;
        m_mediaId = "_LOGO_";

        DisplayLoading();
        ModManager.GetModLogo(modId, logoLocator, logoSize,
                              (t) => OnGetThumbnail(t, modId, "_LOGO_", logoOverlay),
                              WebRequestError.LogAsWarning);
    }

    public void DisplayYouTubeThumbnail(int modId, string youTubeVideoId)
    {
        Debug.Assert(modId > 0,
                     "[mod.io] Mod Id needs to be set to a valid mod profile id.");
        Debug.Assert(!String.IsNullOrEmpty(youTubeVideoId),
                     "[mod.io] youTubeVideoId needs to be set to a valid YouTube video id.");

        m_modId = modId;
        m_mediaId = youTubeVideoId;

        DisplayLoading();
        ModManager.GetModYouTubeThumbnail(modId, youTubeVideoId,
                                          (t) => OnGetThumbnail(t, modId, youTubeVideoId, youTubeOverlay),
                                          WebRequestError.LogAsWarning);
    }

    public void DisplayGalleryImage(int modId, GalleryImageLocator imageLocator)
    {
        Debug.Assert(modId > 0,
                     "[mod.io] Mod Id needs to be set to a valid mod profile id.");
        Debug.Assert(imageLocator != null && !String.IsNullOrEmpty(imageLocator.fileName),
                     "[mod.io] imageLocator needs to be set and have a fileName.");

        m_modId = modId;
        m_mediaId = imageLocator.fileName;

        DisplayLoading();
        ModManager.GetModGalleryImage(modId, imageLocator, galleryImageSize,
                                      (t) => OnGetThumbnail(t, modId, imageLocator.fileName, galleryImageOverlay),
                                      WebRequestError.LogAsWarning);
    }

    public void DisplayLoading()
    {
        image.enabled = false;

        if(loadingPlaceholder != null)
        {
            loadingPlaceholder.SetActive(true);
        }
        if(logoOverlay != null)
        {
            logoOverlay.SetActive(false);
        }
        if(youTubeOverlay != null)
        {
            youTubeOverlay.SetActive(false);
        }
        if(galleryImageOverlay != null)
        {
            galleryImageOverlay.SetActive(false);
        }
    }

    private void OnGetThumbnail(Texture2D texture, int modId, string mediaId, GameObject overlay)
    {
        #if UNITY_EDITOR
        if(!Application.isPlaying) { return; }
        #endif

        if(image == null
           || modId != m_modId
           || mediaId != m_mediaId)
        {
            return;
        }

        if(loadingPlaceholder != null)
        {
            loadingPlaceholder.SetActive(false);
        }
        if(overlay != null)
        {
            overlay.SetActive(true);
        }

        image.sprite = ModBrowser.CreateSpriteFromTexture(texture);
        image.enabled = true;
    }

    public void NotifyClicked()
    {
        // TODO(@jackson)
        throw new System.NotImplementedException();
    }
}
