using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Game { get; private set; }

    private Animator _knightAni;
    private Animator[] _ghostAni;

    private Dictionary<string, int> _aniParam;
    
    private void Awake()
    {
        if (Game == null)
        {
            Game = this;
            DontDestroyOnLoad(Game);
        }
    }

    private void Start()
    {
        _knightAni = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        _ghostAni = GameObject.FindGameObjectsWithTag("Ghost").Select(obj => obj.GetComponent<Animator>()).ToArray();

        _aniParam = new Dictionary<string, int>
        {
            { "Armed", Animator.StringToHash("Armed") },
            { "Moving", Animator.StringToHash("Moving") }
        };
        
        StartCoroutine(Test());
    }
    
    private IEnumerator Test()
    {
        NormalMode();
        AudioManager.PlayMusicOneShot(AudioManager.Audio.musicIntro);
        AudioManager.QueueMusicLoop(AudioManager.Audio.musicNormal);
        yield return new WaitForSecondsRealtime(2);
        Moving(true);
        AudioManager.PlaySfxOneShot(AudioManager.Audio.coin);
        yield return new WaitForSecondsRealtime(10);
        StartCoroutine(ScaredMode());
        AudioManager.PlaySfxOneShot(AudioManager.Audio.potion);
        AudioManager.ImmediateMusicOneShot(AudioManager.Audio.musicScared);
        yield return new WaitForSecondsRealtime(5);
        Moving(false);
        AudioManager.PlaySfxOneShot(AudioManager.Audio.hit);
        yield return new WaitForSecondsRealtime(10);
        AudioManager.PlayMusicLoop(AudioManager.Audio.musicIntermission);
    }

    private void NormalMode()
    {
        _knightAni.SetFloat(_aniParam["Armed"], 0);
        foreach (Animator ani in _ghostAni) ani.SetFloat(_aniParam["Armed"], 1);
    }

    private IEnumerator ScaredMode()
    {
        _knightAni.SetFloat(_aniParam["Armed"], 1);
        foreach (Animator ani in _ghostAni) ani.SetFloat(_aniParam["Armed"], 0);
        yield return new WaitForSeconds(9);
        NormalMode();
    }

    private void Moving(bool moving)
    {
        _knightAni.SetBool(_aniParam["Moving"], moving);
        foreach (Animator ani in _ghostAni) ani.SetBool(_aniParam["Moving"], moving);
    }
}
