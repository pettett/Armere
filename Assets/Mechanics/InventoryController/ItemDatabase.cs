using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Armere.Inventory
{
	public readonly struct ItemDataAsyncSerializer : IBinaryVariableAsyncSerializer<ItemDataAsyncSerializer>
	{
		public readonly ItemData item;

		public ItemDataAsyncSerializer(ItemData item)
		{
			this.item = item;
		}

		public void Read(in GameDataReader reader, Action<ItemDataAsyncSerializer> data)
		{
			ItemDatabase.ReadItemData(reader, item => data?.Invoke(new ItemDataAsyncSerializer(item)));
		}

		public void Write(in GameDataWriter writer) => item.Write(writer);


		public static implicit operator ItemDataAsyncSerializer(ItemData value) => new ItemDataAsyncSerializer(value);
		public static implicit operator ItemData(ItemDataAsyncSerializer value) => value.item;
	}

	[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/ItemDatabase", order = 0)]
	public class ItemDatabase : ScriptableObject
	{

		public static readonly Dictionary<ItemData, (ulong, ulong)> itemDataPrimaryKeys = new Dictionary<ItemData, (ulong, ulong)>();
		public static readonly Dictionary<string, ItemData> itemDataNames = new Dictionary<string, ItemData>();

		public static AsyncOperationHandle<ItemDataT> LoadItemDataAsync<ItemDataT>(AssetReferenceT<ItemDataT> reference, System.Action<ItemDataT> data) where ItemDataT : ItemData
		{
			var x = Addressables.LoadAssetAsync<ItemDataT>(reference);

			Spawner.OnDone(x, result =>
			{
				itemDataPrimaryKeys[result.Result] = GetPrimaryKey(reference);
				itemDataNames[result.Result.displayName] = result.Result;
				data?.Invoke(result.Result);
			});

			return x;
		}


		//Will only work in situations where it can be guaranteed that the item has already been loaded before
		public static ItemData LoadItemData(AssetReferenceT<ItemData> reference)
		{
			return reference.LoadAssetAsync<ItemData>().Result;
		}
		public static AssetReferenceT<ItemData> ReadItemDataReference(in GameDataReader reader)
		{
			ulong a = reader.ReadULong();
			ulong b = reader.ReadULong();
			string guid = Decode(a, b);

			//Debug.Log($"{guid}, {a:x16}{b:x16}");
			return new AssetReferenceT<ItemData>(guid);
		}
		public static void ReadItemData(in GameDataReader reader, System.Action<ItemData> data)
		{

			//Debug.Log($"Reading from {reader.reader.BaseStream.Position }:");
			var x = ReadItemDataReference(in reader);
			//Debug.Log(x.AssetGUID);
			Assert.IsTrue(x.RuntimeKeyIsValid());
			LoadItemDataAsync(x, data);
		}


		public static (ulong, ulong) GetPrimaryKey(AssetReference reference)
		{
			ulong a = ulong.Parse(reference.AssetGUID.Substring(0, 16), System.Globalization.NumberStyles.HexNumber);
			ulong b = ulong.Parse(reference.AssetGUID.Substring(16, 16), System.Globalization.NumberStyles.HexNumber);

			//Debug.Log($"{reference.AssetGUID}, {a:x16}{b:x16}");
			return (a, b);
		}
		static string Decode(ulong a, ulong b)
		{
			return $"{a:x16}{b:x16}";
		}

		[MyBox.ButtonMethod]
		public void Test()
		{
			string start = "d10c68d57a6c39544bb77206dfed0024";
			ulong a = ulong.Parse(start.Substring(0, 16), System.Globalization.NumberStyles.HexNumber);
			ulong b = ulong.Parse(start.Substring(16, 16), System.Globalization.NumberStyles.HexNumber);
			//Debug.Log($"{a:x16}{b:x16}" == start);
		}

	}

}