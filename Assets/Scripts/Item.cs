using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Components:")]
    [SerializeField] private Animator ani;
    public Type type;

    public enum Type
    {
        Coin,
        Potion,
        Ingot
    }

    private void Start()
    {
        GameManager.RegisterItem(this);
        ani.SetFloat("Type", (int) type); // Inefficient to use string name but only done once in lifetime
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.Equals(GameManager.GetCharacter("Player").gameObject)) return;
        switch (type)
        {
            case Type.Coin:
                GameManager.Game.StartCoroutine(GameManager.AddScore(10));
                AudioManager.PlaySfxOneShot(AudioManager.Audio.coin);
                break;
            case Type.Ingot:
                GameManager.Game.StartCoroutine(GameManager.AddScore(100));
                AudioManager.PlaySfxOneShot(AudioManager.Audio.select);
                var player = GameManager.GetCharacter("Player");
                if (player is null) break;
                ((PacStudentController)player).CanPhase = true;
                break;
            case Type.Potion:
                GameManager.GameMode = GameManager.Mode.Scared;
                AudioManager.PlaySfxOneShot(AudioManager.Audio.potion);
                break;
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        GameManager.UnregisterItem(this);
    }
}
