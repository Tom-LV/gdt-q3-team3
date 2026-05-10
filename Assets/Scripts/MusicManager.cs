using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    // Singleton instance so triggers can easily access it
    public static MusicManager Instance;

    [Header("Settings")]
    [Tooltip("How many seconds it takes to fade between tracks.")]
    public float fadeDuration = 2f;
    [Tooltip("The maximum volume for the music (0.0 to 1.0).")]
    public float maxVolume = 1f;

    [Header("Audio mixer")]
    [SerializeField] private AudioMixerGroup audioMixerGroup;

    // We need two sources to crossfade between them
    private AudioSource source1;
    private AudioSource source2;

    // Keep track of what volume each source should be trying to reach
    private float targetVolume1 = 0f;
    private float targetVolume2 = 0f;

    // Tracks which source is currently the "main" one
    private bool isSource1Active = true;

    private void Awake()
    {
        // Standard Singleton setup
        if (Instance == null)
        {
            Instance = this;
            // Optional: DontDestroyOnLoad(gameObject); if you want music to persist between scene loads
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create the two AudioSources dynamically so you don't have to set them up manually
        source1 = gameObject.AddComponent<AudioSource>();
        source2 = gameObject.AddComponent<AudioSource>();

        source1.outputAudioMixerGroup = audioMixerGroup;
        source2.outputAudioMixerGroup = audioMixerGroup;

        source1.loop = true;
        source2.loop = true;

        source1.volume = 0f;
        source2.volume = 0f;
    }

    public void PlayMusic(AudioClip newClip)
    {
        if (newClip == null) return;

        // Determine which source is currently active and which is fading out
        AudioSource activeSource = isSource1Active ? source1 : source2;

        // If the requested clip is already playing on the active source, do nothing!
        if (activeSource.clip == newClip) return;

        // Swap the active source flag
        isSource1Active = !isSource1Active;

        // Set up the new active source and the old fading source
        AudioSource newActiveSource = isSource1Active ? source1 : source2;
        AudioSource fadingSource = isSource1Active ? source2 : source1;

        // Assign the new clip and start playing it (volume is still 0 at this exact frame)
        newActiveSource.clip = newClip;
        newActiveSource.Play();

        // Set the targets. The Update loop will handle the actual volume changes.
        if (isSource1Active)
        {
            targetVolume1 = maxVolume;
            targetVolume2 = 0f;
        }
        else
        {
            targetVolume1 = 0f;
            targetVolume2 = maxVolume;
        }
    }

    private void Update()
    {
        // Calculate how much volume changes this frame
        float fadeSpeed = maxVolume / fadeDuration;

        // Smoothly move both sources towards their target volumes
        source1.volume = Mathf.MoveTowards(source1.volume, targetVolume1, fadeSpeed * Time.deltaTime);
        source2.volume = Mathf.MoveTowards(source2.volume, targetVolume2, fadeSpeed * Time.deltaTime);

        // Turn off the AudioSource completely if it has reached 0 volume to save processing power
        if (source1.volume == 0f && source1.isPlaying && targetVolume1 == 0f) source1.Stop();
        if (source2.volume == 0f && source2.isPlaying && targetVolume2 == 0f) source2.Stop();
    }
}