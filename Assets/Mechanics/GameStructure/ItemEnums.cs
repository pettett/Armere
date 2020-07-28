//I doubt that more then 65,536 items will be defined
public enum ItemName : ushort
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
}
//I doubt that more then 256 item types will be defined
public enum ItemType : byte
{
    Common = 0,
    Weapon = 1,
    Bow = 2,
    Ammo = 3,
    SideArm = 4,
    Quest = 5,
    Currency = 6
}