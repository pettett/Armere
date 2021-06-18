using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Armere.Inventory
{
	[CreateAssetMenu(menuName = "Saving/Test")]
	public class TestRegion : SaveableSO
	{
		public ItemData data;
		public uint count = 10;
		public override void LoadBin(in GameDataReader reader)
		{
			Debug.Log(reader.ReadInt());
			Debug.Log(reader.ReadUInt());
			reader.ReadAsync<ItemStack>(x =>
						{
							Debug.Log(x);
						});
		}

		public override void LoadBlank()
		{
		}

		public override void SaveBin(in GameDataWriter writer)
		{
			writer.WritePrimitive(50);
			writer.WritePrimitive(count);

			ItemStack s = new ItemStack(data, count);
			writer.Write(s);
		}

	}
}