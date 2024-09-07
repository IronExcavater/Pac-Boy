using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Audio { get; private set; }
    
    [Header("Audio Sources:")]
    [SerializeField] private AudioSource[] musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips:")]
    public AudioClipTempoTuple musicMenu;
    public AudioClipTempoTuple musicIntro;
    public AudioClipTempoTuple musicNormal;
    public AudioClipTempoTuple musicScared;
    public AudioClipTempoTuple musicIntermission;
    
    [Header("SFX Clips:")]
    public AudioClip step;
    public AudioClip coin;
    public AudioClip potion;
    public AudioClip hit;
    public AudioClip defeat;
    public AudioClip score;
    public AudioClip select;

    private AudioClipTempoTuple _loopedMusic;
    private int _sourceToggle;
    private double _endDspTime;

    private void Awake()
    {
        if (Audio == null)
        {
            Audio = this;
            DontDestroyOnLoad(Audio);
        }
        else Destroy(this);
    }
    
    private void Update()
    {
        LoopMusic();
    }

    private void LoopMusic()
    {
        if (AudioSettings.dspTime < _endDspTime - 1) return;
        if (_loopedMusic is null) return;
        
        AudioClip nextClip = _loopedMusic.AudioClip;
        // Load next clip and schedule at end time
        musicSource[_sourceToggle].clip = nextClip;
        musicSource[_sourceToggle].PlayScheduled(_endDspTime);
        // Add next clip's duration to end time
        _endDspTime += (double) nextClip.samples / nextClip.frequency;
        // Switch source toggle
        _sourceToggle = 1 - _sourceToggle;
    }

    public static void PlayMusicOneShot(AudioClipTempoTuple clip)
    {
        Audio.ScheduleMusicClip(clip, Audio.NextBeat());
    }

    public static void ImmediateMusicOneShot(AudioClipTempoTuple clip)
    {
        Audio.ScheduleMusicClip(clip, AudioSettings.dspTime);
    }
    
    public static void PlayMusicLoop(AudioClipTempoTuple clip)
    {
        Audio.ScheduleMusicClip(clip, Audio.NextBar());
        Audio._loopedMusic = clip;
    }

    public static void QueueMusicLoop(AudioClipTempoTuple clip)
    {
        Audio._loopedMusic = clip;
    }
    
    public static void PlaySfxOneShot(AudioClip clip)
    {
        Audio.sfxSource.PlayOneShot(clip);
    }

    private double NextBar()
    {
        if (_loopedMusic == null) return AudioSettings.dspTime;
        // Calculate next musical bar time of current clip
        double barDuration = 60d / _loopedMusic.Tempo * 4; // minute / bpm * time signature (assumed 4/4)
        double clipElapsedDspTime = (double) musicSource[1 - _sourceToggle].timeSamples / _loopedMusic.AudioClip.frequency;
        return AudioSettings.dspTime + barDuration - clipElapsedDspTime % barDuration;
    }

    private double NextBeat()
    {
        if (_loopedMusic == null) return AudioSettings.dspTime;
        // Calculate next musical beat time of current clip
        double beatDuration = 60d / _loopedMusic.Tempo;
        double clipElapsedDspTime = (double) musicSource[1 - _sourceToggle].timeSamples / _loopedMusic.AudioClip.frequency;
        return AudioSettings.dspTime + beatDuration - clipElapsedDspTime % beatDuration;
    }

    private void ScheduleMusicClip(AudioClipTempoTuple clip, double nextDspTime)
    {
        // Load next clip and schedule at next time
        musicSource[_sourceToggle].clip = clip.AudioClip;
        musicSource[_sourceToggle].PlayScheduled(nextDspTime);
        // Set end time to next time plus clip's duration
        if (_loopedMusic != null) musicSource[1 - _sourceToggle].SetScheduledEndTime(nextDspTime);
        _endDspTime = nextDspTime + (double) clip.AudioClip.samples / clip.AudioClip.frequency;
        // Switch source toggle
        _sourceToggle = 1 - _sourceToggle;
    }
}
