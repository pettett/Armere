using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/ItemDatabase", order = 0)]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public class ItemData
    {
        public ItemName itemName;
        public string name;
        [TextArea]
        public string description;
        public uint sellValue = 1;
        public bool staticPickup;
        public ItemType type;
        public Sprite sprite;
        public Mesh mesh;
        public Material[] materials;
        public ItemPropertyBase properties;
    }

    public ItemData this[ItemName key] => itemData[(int)key];

    public ItemData[] itemData;

}