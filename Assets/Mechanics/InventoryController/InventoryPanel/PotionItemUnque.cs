
namespace Armere.Inventory
{


	public class PotionItemUnique : ItemStackBase
	{

		public float potency;
		public float duration;


		//This should never be called
		public PotionItemUnique() : base(ItemName.EmptyPotion)
		{
		}

		public PotionItemUnique(ItemName name) : base(name)
		{
		}

		public PotionItemUnique(ItemName name, float potency, float duration) : base(name)
		{
			this.potency = potency;
			this.duration = duration;
		}


		public override void Write(GameDataWriter writer)
		{
			writer.Write((int)name);
			writer.Write(potency);
			writer.Write(duration);
		}

	}
}