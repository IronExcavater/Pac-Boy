using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Components:")]
    [SerializeField] private Animator ani;
    [SerializeField] private Type type;

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
                GameManager.AddScore(10);
                break;
            case Type.Ingot:
                GameManager.AddScore(100);
                break;
            case Type.Potion:
                GameManager.GameMode = GameManager.Mode.Scared;
                break;
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        GameManager.UnregisterItem(this);
    }
}
