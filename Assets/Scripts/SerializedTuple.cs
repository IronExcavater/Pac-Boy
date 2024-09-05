using System;
using UnityEngine;

[Serializable]
public class AudioClipTempoTuple : Tuple<AudioClip, int>
{
    [SerializeField] protected AudioClip audioClip;
    [SerializeField] protected int tempo;
    
    public AudioClip AudioClip
    {
        get => audioClip;
        set => audioClip = value;
    }

    public int Tempo
    {
        get => tempo;
        set => tempo = value;
    }
    
    public AudioClipTempoTuple(AudioClip audioClip, int tempo) : base(audioClip, tempo)
    {
        this.audioClip = audioClip;
        this.tempo = tempo;
    }
}