using System.Collections;
using System.Collections.Generic;
using Armere.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Armere.Inventory
{

	[CreateAssetMenu(menuName = "Game/Items/Item Data", fileName = "New Item Data")]
	[AllowItemTypes(ItemType.Common, ItemType.Quest, ItemType.Currency, ItemType.Potion)]
	public class ItemData : ScriptableObject
	{
		public ItemName itemName;
		public ItemType type;
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
		public PotionItemName potionWorksFrom;
		[MyBox.ConditionalField("potionIngredient")]
		public bool changePotionType;
		[MyBox.ConditionalField("potionIngredient")]
		public PotionItemName newPotionType;

		[Tooltip("If changing potion type, this will be the base duraction"), MyBox.ConditionalField("potionIngredient")]
		public float increasedDuration;

		[Tooltip("If changing potion type, this will be the base potency"), MyBox.ConditionalField("potionIngredient")]
		public float increasedPotency;
		[Header("Interaction")]
		[Tooltip("Should be none for most normal items")]
		public ItemInteractionCommands disabledCommands;


	}
}