using UnityEngine;

public class Key : Item
{
    [SerializeField] private string id;
    string GetID()
    {
        return id;
    }
}
