using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/ItemDatabase", order = 0)]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public class ItemData
    {
        public string name;
        [TextArea]
        public string description;
        public uint sellValue = 1;
        public bool staticPickup;
        public ItemType type;
        public Sprite sprite;
        public Mesh mesh;
        public Material[] materials;
        public ItemPropertyBase[] properties;
    }
    [System.Serializable]
    public class ItemDatabaseStructure : RotaryHeart.Lib.SerializableDictionary.SerializableDictionaryBase<ItemName, ItemData> { }
    public ItemDatabaseStructure itemDatabase;

    public ItemData this[ItemName key] => itemDatabase[key];

}