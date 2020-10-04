//This needs to be contiguous - values used as index
using System.Collections.Generic;

public enum ItemName
{
    Stick = 0,
    MineDeed = 1,
    IronSword = 2,
    Lantern = 3,
    Meat = 4,
    Herb = 5,
    IronOre = 6,
    MagicTome = 7,
    DungeonKey = 8,
    BasicBow = 9,
    Arrow = 10,
    Currency = 11,
    BasicShield = 12
}

public enum ItemType
{
    Common = 0,
    Melee = 1,
    Bow = 2,
    Ammo = 3,
    SideArm = 4,
    Quest = 5,
    Currency = 6
}



public class ItemTypeEqualityComparer : IEqualityComparer<ItemType>
{
    public bool Equals(ItemType t1, ItemType t2)
    {
        return t1 == t2;
    }

    public int GetHashCode(ItemType t)
    {
        return (int)t;
    }
}