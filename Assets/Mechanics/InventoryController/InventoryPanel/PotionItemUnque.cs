
namespace Armere.Inventory
{


	public class PotionItemUnique : ItemStackBase, IGameDataSavableAsync<PotionItemUnique>
	{

		public float potency;
		public float duration;


		public PotionItemUnique()
		{
		}

		public PotionItemUnique(float potency, float duration)
		{
			this.potency = potency;
			this.duration = duration;
		}

		public PotionItemUnique(ItemData item, float potency, float duration) : base(item)
		{
			this.potency = potency;
			this.duration = duration;
		}


		public void Read(in GameDataReader reader, System.Action<PotionItemUnique> onDone)
		{
			float potency = reader.ReadFloat();
			float duration = reader.ReadFloat();
			reader.ReadAsync<ItemDataAsyncSerializer>(item =>
			{
				onDone?.Invoke(new PotionItemUnique(item, potency, duration));
			});
		}

		public override void Write(in GameDataWriter writer)
		{
			writer.WritePrimitive(potency);
			writer.WritePrimitive(duration);
			base.Write(in writer);
		}

		PotionItemUnique IGameDataSerializable<PotionItemUnique>.Init()
		{
			return this;
		}
	}
}