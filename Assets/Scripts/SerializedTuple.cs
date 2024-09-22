using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class AudioClipTempoTuple
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private int tempo;

    public AudioClip AudioClip => audioClip;
    public int Tempo => tempo;

    public AudioClipTempoTuple(AudioClip audioClip, int tempo)
    {
        this.audioClip = audioClip;
        this.tempo = tempo;
    }
}