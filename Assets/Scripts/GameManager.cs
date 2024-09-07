using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Game { get; private set; }

    private List<Character> _characters = new();

    public enum Mode
    {
        Normal,
        Scared,
        Victory,
        Defeat
    }
    
    private void Awake()
    {
        if (Game == null)
        {
            Game = this;
            DontDestroyOnLoad(Game);
        }
        else Destroy(this);
    }

    private void Start()
    {
        StartCoroutine(Test());
    }
    
    private IEnumerator Test()
    {
        TriggerMode(Mode.Normal);
        AudioManager.PlayMusicOneShot(AudioManager.Audio.musicIntro);
        AudioManager.QueueMusicLoop(AudioManager.Audio.musicNormal);
        yield return new WaitForSecondsRealtime(2);
        AudioManager.PlaySfxOneShot(AudioManager.Audio.coin);
        yield return new WaitForSecondsRealtime(10);
        TriggerMode(Mode.Scared);
        AudioManager.PlaySfxOneShot(AudioManager.Audio.potion);
        AudioManager.ImmediateMusicOneShot(AudioManager.Audio.musicScared);
        yield return new WaitForSecondsRealtime(5);
        AudioManager.PlaySfxOneShot(AudioManager.Audio.hit);
        yield return new WaitForSecondsRealtime(10);
        AudioManager.PlayMusicLoop(AudioManager.Audio.musicIntermission);
    }

    public static void RegisterCharacter(Character character)
    {
        if (!Game._characters.Contains(character)) Game._characters.Add(character);
    }

    public static void UnregisterCharacter(Character character)
    {
        Game._characters.Remove(character);
    }

    private static void TriggerMode(Mode mode)
    {
        foreach (var character in Game._characters) character.ChangeMode(mode);

        if (mode.Equals(Mode.Scared)) Game.StartCoroutine(TriggerMode(Mode.Normal, 9f));
    }

    public static IEnumerator TriggerMode(Mode mode, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        TriggerMode(mode);
    }
}
