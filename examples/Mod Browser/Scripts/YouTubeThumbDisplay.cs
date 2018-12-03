using System;
using UnityEngine;
using UnityEngine.UI;
using ModIO;

public class YouTubeThumbDisplay : MonoBehaviour
{
    // ---------[ FIELDS ]---------
    public delegate void OnClickDelegate(YouTubeThumbDisplay component,
                                         int modId, string youTubeVideoId);
    public event OnClickDelegate onClick;

    [Header("UI Components")]
    public Image image;
    public GameObject loadingDisplay;

    [Header("Display Data")]
    [SerializeField] private int m_modId;
    [SerializeField] private string m_youTubeVideoId;

    // ---------[ INITIALIZATION ]---------
    public void Initialize()
    {
        Debug.Assert(image != null);
    }

    // ---------[ UI FUNCTIONALITY ]---------
    public void DisplayYouTubeThumbnail(int modId, string youTubeVideoId)
    {
        Debug.Assert(modId > 0,
                     "[mod.io] Mod Id needs to be set to a valid mod profile id.");
        Debug.Assert(!String.IsNullOrEmpty(youTubeVideoId),
                     "[mod.io] youTubeVideoId needs to be set to a valid YouTube video id.");

        DisplayLoading();

        m_modId = modId;
        m_youTubeVideoId = youTubeVideoId;

        ModManager.GetModYouTubeThumbnail(modId, youTubeVideoId,
                                          (t) => LoadTexture(t, youTubeVideoId),
                                          WebRequestError.LogAsWarning);
    }

    public void DisplayTexture(int modId, string youTubeVideoId, Texture2D texture)
    {
        Debug.Assert(modId > 0, "[mod.io] Mod Id needs to be set to a valid mod profile id.");
        Debug.Assert(!String.IsNullOrEmpty(youTubeVideoId),
                     "[mod.io] youTubeVideoId needs to be set to a valid YouTube video id.");
        Debug.Assert(texture != null);

        m_modId = modId;
        m_youTubeVideoId = youTubeVideoId;

        LoadTexture(texture, youTubeVideoId);
    }

    public void DisplayLoading(int modId = -1)
    {
        m_modId = modId;

        if(loadingDisplay != null)
        {
            loadingDisplay.SetActive(true);
        }

        image.enabled = false;
    }

    private void LoadTexture(Texture2D texture, string youTubeVideoId)
    {
        #if UNITY_EDITOR
        if(!Application.isPlaying) { return; }
        #endif

        if(youTubeVideoId != m_youTubeVideoId
           || this.image == null)
        {
            return;
        }

        if(loadingDisplay != null)
        {
            loadingDisplay.SetActive(false);
        }

        image.sprite = ModBrowser.CreateSpriteFromTexture(texture);
        image.enabled = true;
    }

    // ---------[ EVENT HANDLING ]---------
    public void NotifyClicked()
    {
        if(this.onClick != null)
        {
            this.onClick(this, m_modId, m_youTubeVideoId);
        }
    }

    // ---------[ UTILITIES ]---------
    public void OpenYouTubeVideoURL()
    {
        ModBrowser.OpenYouTubeVideoURL(m_youTubeVideoId);
    }
}
