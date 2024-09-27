using System.Collections;
using NUnit.Framework.Internal;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources:")]
    [SerializeField] private AudioSource[] musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips:")]
    [SerializeField] private AudioClipTempoTuple musicMenu;
    [SerializeField] private AudioClipTempoTuple musicIntro;
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
    [SerializeField] private AudioClip ghostDeath;

    private AudioClipTempoTuple _loopedMusic;
    private int _sourceToggle;
    private double _endDspTime;

    private void Start()
    {
        StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        PlayMusicOneShot(musicIntro);
        QueueMusicLoop(musicNormal);
        yield return new WaitForSecondsRealtime(2);
        PlaySfxOneShot(coin);
        yield return new WaitForSecondsRealtime(10);
        PlaySfxOneShot(potion);
        ImmediateMusicOneShot(musicScared);
        yield return new WaitForSecondsRealtime(5);
        PlaySfxOneShot(hit);
        yield return new WaitForSecondsRealtime(10);
        PlayMusicLoop(musicIntermission);
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

    public void PlayMusicOneShot(AudioClipTempoTuple clip)
    {
        ScheduleMusicClip(clip, NextBeat());
    }

    public void ImmediateMusicOneShot(AudioClipTempoTuple clip)
    {
        ScheduleMusicClip(clip, AudioSettings.dspTime);
    }
    
    public void PlayMusicLoop(AudioClipTempoTuple clip)
    {
        ScheduleMusicClip(clip, NextBar());
        _loopedMusic = clip;
    }

    public void QueueMusicLoop(AudioClipTempoTuple clip)
    {
        _loopedMusic = clip;
    }
    
    public void PlaySfxOneShot(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
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
