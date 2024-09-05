using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources:")]
    [SerializeField] private AudioSource[] musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips:")]
    [SerializeField] private AudioClipTempoTuple musicMenu;
    [SerializeField] private AudioClipTempoTuple musicNormal;
    [SerializeField] private AudioClipTempoTuple musicScared;
    [SerializeField] private AudioClipTempoTuple musicIntermission;
    
    [Header("SFX Clips:")]
    [SerializeField] private AudioClip step;
    [SerializeField] private AudioClip coin;
    [SerializeField] private AudioClip potion;
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip death;
    [SerializeField] private AudioClip score;
    [SerializeField] private AudioClip select;

    private AudioClipTempoTuple _loopedMusic;
    private int _sourceToggle;
    private double _endDspTime;

    private void Start()
    {
        PlayMusicLoop(musicNormal);
    }
    
    private void Update()
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

    public void PlayMusicLoop(AudioClipTempoTuple clip)
    {
        // Calculate next bar time of current clip
        double nextBarDspTime = AudioSettings.dspTime;
        if (_loopedMusic != null)
        {
            double barDuration = 60d / _loopedMusic.Tempo * 4; // minute / bpm * time signature
            double clipElapsedDspTime = (double)musicSource[1 - _sourceToggle].timeSamples / _loopedMusic.AudioClip.frequency;
            nextBarDspTime += barDuration - clipElapsedDspTime % barDuration;
            musicSource[1 - _sourceToggle].SetScheduledEndTime(nextBarDspTime);
        }
        // Load next clip and schedule at next bar
        musicSource[_sourceToggle].clip = clip.AudioClip;
        musicSource[_sourceToggle].PlayScheduled(nextBarDspTime);
        // Set end time to next bar plus clip's duration
        _endDspTime = nextBarDspTime + (double) clip.AudioClip.samples / clip.AudioClip.frequency;
        // Switch source toggle and set looped music to clip
        _sourceToggle = 1 - _sourceToggle;
        _loopedMusic = clip;
    }

    public void PlayMusicOneShot(AudioClipTempoTuple clip)
    {
        musicSource[1 - _sourceToggle].PlayOneShot(clip.AudioClip);
    }

    public void PlaySfxOneShot(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
