
namespace Armere.Inventory
{


	public class PotionItemUnique : ItemStackBase
	{

		public float potency;
		public float duration;


		public PotionItemUnique(ItemData item) : base(item)
		{
		}

		public PotionItemUnique(ItemData item, float potency, float duration) : base(item)
		{
			this.potency = potency;
			this.duration = duration;
		}


		public override void Write(GameDataWriter writer)
		{
			writer.Write((int)item.itemName);
			writer.Write(potency);
			writer.Write(duration);
		}

	}
}