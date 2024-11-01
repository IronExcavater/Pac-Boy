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
    [SerializeField] private GameUIController uiController;
    
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
    public float time;
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
        StartCoroutine(StartLevel(0));
    }

    private void Start()
    {
        AudioManager.PlayMusicLoopNextBar(AudioManager.Audio.musicIntro);
    }

    public static IEnumerator StartLevel(float delaySeconds)
    {
        GameMode = Mode.None;
        yield return new WaitUntil(() => !LoadManager.isLoading);
        yield return new WaitForSeconds(delaySeconds);
        Game.countdownTime = Time.time;
        Game.uiController.ShowCountdown();
        foreach (var character in Game._characters.Values)
        {
            character.Reset();
            character.Lock();
        }
        yield return new WaitUntil(() => Game.countdownLength - (Time.time - Game.countdownTime) < 0);
        Game.uiController.HideCountdown();
        foreach (var character in Game._characters.Values)
            character.Unlock();
        GameMode = Mode.Chase;
    }
    
    public static IEnumerator FinishLevel(float delaySeconds)
    {
        GameMode = Mode.None;
        yield return new WaitForSeconds(delaySeconds);
        Game.uiController.ShowGameOver();
        LoadManager.SaveHighScore(Game.time, Game.score);
        yield return new WaitForSeconds(3);
        LoadManager.LoadScene("StartScene");
    }

    public static void RestartLevel(float delaySeconds)
    {
        Game.lives -= 1;
        Game.StartCoroutine(Game.lives <= 0 ? FinishLevel(delaySeconds) : StartLevel(delaySeconds));
    }

    public void Update()
    {
        if (GameMode != Mode.None) time += Time.deltaTime;
    }

    public static void RegisterCharacter(string identifier, Character character)
    {
        var result = Game._characters.TryAdd(identifier, character);
        if (result)
        {
            character.Reset();
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

    public static void CheckForCoins()
    {
        foreach (var item in Game._items)
            if (item.type == Item.Type.Coin)
                return;
        Game.StartCoroutine(FinishLevel(3));
    }

    public static void CheckForDeadGhosts()
    {
        foreach (var character in Game._characters.Values)
            if (character is GhostController { IsAlive: false })
                return;
        if (AudioManager.IsCurrentClip(AudioManager.Audio.musicGhost.AudioClip))
            AudioManager.PlayMusicLoopNextBar(AudioManager.Audio.musicNormal);
    }

    public static IEnumerator AddScore(int score)
    {
        for (var i = 0; i < score / 10; i++)
        {
            Game.score += 10;
            yield return new WaitForSeconds(0.2f);
        }
        Game.score += score % 10;
    }

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
                if (!AudioManager.IsCurrentClip(AudioManager.Audio.musicGhost.AudioClip))
                    AudioManager.PlayMusicLoopNextBar(AudioManager.Audio.musicNormal);
                break;
            case Mode.Scared:
                AudioManager.PlayMusicOneShotNow(AudioManager.Audio.musicScared);
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
