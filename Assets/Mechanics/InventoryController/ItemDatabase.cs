using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

namespace Armere.Inventory
{

	[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/ItemDatabase", order = 0)]
	public class ItemDatabase : SaveableSO
	{
		public ItemData this[ItemName key] => itemData[(int)key];
		public AssetReferenceT<ItemData> test;
		public ItemData[] itemData;

		public Dictionary<ItemData, (ulong, ulong)> itemDataPrimaryKeys = new Dictionary<ItemData, (ulong, ulong)>();

		public void LoadItemData(AssetReferenceT<ItemData> reference, System.Action<ItemData> data)
		{
			var x = reference.LoadAssetAsync<ItemData>();

			Debug.Log(reference.RuntimeKey);

			Spawner.OnDone(x, result =>
			{
				itemDataPrimaryKeys[result.Result] = Encode(reference);
				data?.Invoke(result.Result);
			});
		}
		public void SaveItemData(ItemData data, in GameDataWriter writer)
		{
			(ulong, ulong) value = itemDataPrimaryKeys[data];
			writer.Write(value.Item1);
			writer.Write(value.Item2);
		}
		public AssetReferenceT<ItemData> ReadItemDataReference(in GameDataReader reader)
		{
			(ulong a, ulong b) = (reader.ReadULong(), reader.ReadULong());
			return new AssetReferenceT<ItemData>(Decode(a, b));
		}
		public void ReadItemData(in GameDataReader reader, System.Action<ItemData> data)
		{
			var x = ReadItemDataReference(in reader);
			LoadItemData(x, data);
		}


		(ulong, ulong) Encode(AssetReference reference)
		{

			ulong a = ulong.Parse(reference.AssetGUID.Substring(0, 16), System.Globalization.NumberStyles.HexNumber);
			ulong b = ulong.Parse(reference.AssetGUID.Substring(16, 16), System.Globalization.NumberStyles.HexNumber);

			return (a, b);
		}
		string Decode(ulong a, ulong b)
		{
			return $"{a:X16}{b:X16}";
		}


		public void Test()
		{

			LoadItemData(test, null);

		}

		public override void SaveBin(in GameDataWriter writer)
		{

		}

		public override void LoadBin(in GameDataReader reader)
		{

		}

		public override void LoadBlank()
		{

		}
	}
}