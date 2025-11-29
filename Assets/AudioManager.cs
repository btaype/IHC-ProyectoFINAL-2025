using UnityEngine;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    [Header("Sound Effects")]
    public AudioClip jumpSound;
    // public AudioClip startGameSound;
    // public AudioClip damageSound;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
            return;
        }

        // Create two AudioSources
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        // Background music setup
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
    }

    void Start()
    {
        PlayMusic();
    }

    // -----------------------
    // MUSIC METHODS
    // -----------------------
    public void PlayMusic()
    {
        if (backgroundMusic != null)
            musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // -----------------------
    // SFX METHODS
    // -----------------------
    public void PlayJumpSound()
    {
        if (jumpSound != null)
            sfxSource.PlayOneShot(jumpSound);
    }

    // public void PlayStartGameSound()
    // {
    //     if (startGameSound != null)
    //         sfxSource.PlayOneShot(startGameSound);
    // }

    // public void PlayDamageSound()
    // {
    //     if (damageSound != null)
    //         sfxSource.PlayOneShot(damageSound);
    // }
}