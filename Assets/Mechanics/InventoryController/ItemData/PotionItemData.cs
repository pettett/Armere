using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.Inventory
{
	public enum PotionEffect
	{
		Health
	}
	public class PotionItemData : ItemData
	{
		public PotionEffect effect;
	}
}
