using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Game { get; private set; }
    
    [Header("References:")]
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase fillerTile;
    
    private Tilemap _map;
    private Dictionary<string, Character> _characters = new();
    private List<Item> _items = new();

    private HashSet<string> _logIdentifiers = new();

    [Header("Attributes:")]
    public float characterSpeed;
    public float scaredSpeed;
    public float scaredLength;
    public float scatterLength;
    public float countdownLength;
    [SerializeField] private Mode mode;
    
    [Header("Game:")]
    public int score;
    public float startTime;
    public float endTime;
    public float lives = 3;
    public float scaredTime;
    public float scatterTime;
    public float countdownTime;

    public enum Mode
    {
        None,
        Chase,
        Scatter,
        Scared
    }
    
    private void Awake()
    {
        Game = this;
        _map = GameObject.Find("Level01").GetComponent<Tilemap>();
        _map.CompressBounds();
        var cam = Camera.main;
        if (cam == null) return;
        cam.orthographicSize = _map.cellBounds.size.y / 2f;
        cam.transform.position = _map.cellBounds.center - new Vector3(5.5f, 0.5f, 10);
        StartCoroutine(StartLevel(2));
    }

    public IEnumerator StartLevel(float delaySeconds)
    {
        yield return new WaitUntil(() => !LoadManager.isLoading);
        yield return new WaitForSeconds(delaySeconds);
        countdownTime = Time.time;
        foreach (var character in Game._characters.Values)
            character.Spawn();
        yield return new WaitUntil(() => countdownLength - (Time.time - countdownTime) < 0);
        foreach (var character in Game._characters.Values)
            character.Unlock();
        startTime = Time.time;
        GameMode = Mode.Chase;
    }

    public static void RestartLevel(float delaySeconds)
    {
        Game.lives -= 1;
        GameMode = Mode.None;
        Game.StartCoroutine(Game.StartLevel(delaySeconds));
    }

    public static float ElapsedTime()
    {
        if (GameMode != Mode.None) Game.endTime = Time.time;
        return Game.endTime - Game.startTime;
    }

    public static void RegisterCharacter(string identifier, Character character)
    {
        var result = Game._characters.TryAdd(identifier, character);
        if (result)
        {
            character.Spawn();
            character.TriggerMode();
            return;
        }
        Debug.LogWarning("Character with identifier " + identifier + " is already registered.");
    }

    public static void UnregisterCharacter(string identifier)
    {
        Game._characters.Remove(identifier);
    }
    
    public static Character GetCharacter(string identifier)
    {
        if (Game._characters.TryGetValue(identifier, out var character)) return character;
        LogWarning("No character found with identifier: " + identifier, "No " + identifier);
        return null;
    }

    public static void ClearCharacters()
    {
        foreach (var character in Game._characters.Values)
            Destroy(character.gameObject);
    }

    public static void RegisterItem(Item item)
    {
        if (!Game._items.Contains(item)) Game._items.Add(item);
    }

    public static void UnregisterItem(Item item)
    {
        Game._items.Remove(item);
    }

    public static void ClearItems()
    {
        foreach (var item in Game._items)
            Destroy(item.gameObject);
    }

    public static void AddScore(int score) { Game.score += score; }

    public static Tilemap LevelTilemap() { return Game._map; }

    public static bool IsGroundTile(TileBase tile) { return tile != null && tile.Equals(Game.groundTile); }
    
    public static bool IsFillerTile(TileBase tile) { return tile != null && tile.Equals(Game.fillerTile); }

    public static Mode GameMode
    {
        get => Game.mode;
        set
        {
            if (value == Game.mode) return;
            Game.mode = value;
            TriggerMode();
        }
    }

    private static void TriggerMode()
    {
        foreach (var character in Game._characters.Values) character.TriggerMode();

        switch (GameMode)
        {
            case Mode.Chase or Mode.Scatter:
                AudioManager.PlayMusicLoop(AudioManager.Audio.musicNormal);
                break;
            case Mode.Scared:
                AudioManager.PlayMusicImmediate(AudioManager.Audio.musicScared);
                Game.scaredTime = Time.time;
                Game.StartCoroutine(QueueMode(Mode.Chase, Game.scaredLength));
                break;
        }
    }

    private static IEnumerator QueueMode(Mode mode, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        GameMode = mode;
    }

    public static void LogWarning(string message, string logIdentifier)
    {
        if (Game._logIdentifiers.Contains(logIdentifier)) return;
        
        Debug.LogWarning(message);
        Game._logIdentifiers.Add(logIdentifier);
    }
}
