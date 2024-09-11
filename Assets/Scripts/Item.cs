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

    private void OnDestroy()
    {
        GameManager.UnregisterItem(this);
    }
}
