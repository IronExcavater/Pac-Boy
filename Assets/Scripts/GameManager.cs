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
        Chase,
        Scatter,
        Scared
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

        if (mode.Equals(Mode.Scared)) Game.StartCoroutine(TriggerMode(Mode.Chase, 9f));
    }

    private static IEnumerator TriggerMode(Mode mode, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        TriggerMode(mode);
    }
}
