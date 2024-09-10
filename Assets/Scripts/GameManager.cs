using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Game { get; private set; }

    private List<Character> _characters = new();
    private List<Item> _items = new();

    public enum Mode
    {
        Normal,
        Scared,
        // Temp modes
        Direction,
        Move,
        Attack,
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
        AudioManager.PlayMusicOneShot(AudioManager.Audio.musicIntro);
        AudioManager.QueueMusicLoop(AudioManager.Audio.musicNormal);
        yield return new WaitForSeconds(3);
        TriggerMode(Mode.Normal);
        for (var i = 0; i < 3; i++)
        {
            AudioManager.PlaySfxOneShot(AudioManager.Audio.coin);
            yield return new WaitForSeconds(1);
            TriggerMode(Mode.Attack);
            AudioManager.PlaySfxOneShot(AudioManager.Audio.hit);
            yield return new WaitForSeconds(2);
            TriggerMode(Mode.Move);
            yield return new WaitForSeconds(2);
            TriggerMode(Mode.Scared);
            AudioManager.PlaySfxOneShot(AudioManager.Audio.potion);
            AudioManager.ImmediateMusicOneShot(AudioManager.Audio.musicScared);
            yield return new WaitForSeconds(4);
            TriggerMode(Mode.Move);
            yield return new WaitForSeconds(6);
            TriggerMode(Mode.Direction);
            AudioManager.PlaySfxOneShot(AudioManager.Audio.select);
            yield return new WaitForSeconds(2);
        }
        AudioManager.PlayMusicLoop(AudioManager.Audio.musicIntermission);
        yield return new WaitForSeconds(4);
        TriggerMode(Mode.Defeat);
        AudioManager.PlaySfxOneShot(AudioManager.Audio.defeat);
    }

    public static void RegisterCharacter(Character character)
    {
        if (!Game._characters.Contains(character)) Game._characters.Add(character);
    }

    public static void UnregisterCharacter(Character character)
    {
        Game._characters.Remove(character);
    }

    public static void RegisterItem(Item item)
    {
        if (!Game._items.Contains(item)) Game._items.Add(item);
    }

    public static void UnregisterItem(Item item)
    {
        Game._items.Remove(item);
    }

    private static void TriggerMode(Mode mode)
    {
        foreach (var character in Game._characters) character.ChangeMode(mode);

        if (mode.Equals(Mode.Scared)) Game.StartCoroutine(TriggerMode(Mode.Normal, 9f));
    }

    private static IEnumerator TriggerMode(Mode mode, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        TriggerMode(mode);
    }
}
