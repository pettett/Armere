using System.Collections;
using System.Collections.Generic;
using Armere.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Armere.Inventory
{

	[CreateAssetMenu(menuName = "Game/Items/Item Data", fileName = "New Item Data")]
	[AllowItemTypes(ItemType.Common, ItemType.Quest, ItemType.Currency, ItemType.Potion)]
	public class ItemData : ScriptableObject, IGameDataSerializable<ItemData>
	{
		public static implicit operator ItemData(string itemName) => ItemDatabase.itemDataNames[itemName];


		[Header("Item Data")]
		public ItemType type;
		public AssetReferenceT<ItemData> selfReference;
		[UnityEngine.Serialization.FormerlySerializedAs("displaySprite")] public AssetReferenceSprite thumbnail;
		public string displayName = "New Item";
		[TextArea]
		public string description = "This item has no description";
		[Header("Economy")]
		public bool sellable = true;
		public uint sellValue = 25u;
		[Header("Potions")]
		public bool potionIngredient;
		[MyBox.ConditionalField("potionIngredient")]
		public ItemData potionWorksFrom;
		[MyBox.ConditionalField("potionIngredient")]
		public bool changePotionType;
		[MyBox.ConditionalField("potionIngredient")]
		public PotionItemData newPotionType;

		[Tooltip("If changing potion type, this will be the base duraction"), MyBox.ConditionalField("potionIngredient")]
		public float increasedDuration;

		[Tooltip("If changing potion type, this will be the base potency"), MyBox.ConditionalField("potionIngredient")]
		public float increasedPotency;
		[Header("Interaction")]
		[Tooltip("Should be none for most normal items")]
		public ItemInteractionCommands disabledCommands;


		public void Write(in GameDataWriter writer)
		{
			(ulong, ulong) value = ItemDatabase.itemDataPrimaryKeys[this];
			writer.WritePrimitive(value.Item1);
			writer.WritePrimitive(value.Item2);
		}

		public ItemData Init()
		{
			return this;
		}
	}
}