namespace Armere.Inventory
{
	[System.Serializable]
	public struct BuyMenuItem
	{
		public ItemData item;
		public uint count;
		public uint stock;
		public uint cost;
	}
}