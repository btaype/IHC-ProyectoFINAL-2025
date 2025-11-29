using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoButtonController : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Referencia al componente VideoPlayer
    public Button playButton; // Referencia al botón de la UI
    public GameObject menu; // Referencia al GameObject del menú (por ejemplo, el Canvas del menú)

    void Start()
    {
        videoPlayer.enabled = false;
        videoPlayer.Stop();

        videoPlayer.loopPointReached -= EndReached;

        playButton.onClick.AddListener(PlayVideo);

        videoPlayer.loopPointReached += EndReached;
    }

    public void PlayVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.enabled = true;
            videoPlayer.Play();

            if (menu != null)
            {
                menu.SetActive(false);
            }
        }
    }

    void EndReached(VideoPlayer vp)
    {
        videoPlayer.Stop();
        videoPlayer.enabled = false;

        if (menu != null)
        {
            menu.SetActive(true);
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= EndReached;
        }
    }

    // 👇 NUEVO MÉTODO para detener el video desde el reconocimiento de voz
    public void StopVideoFromVoice()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            videoPlayer.enabled = false;

            if (menu != null)
            {
                menu.SetActive(true);
            }

            Debug.Log("Video detenido por comando de voz.");
        }
    }
}
