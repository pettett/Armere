using System.Collections;
using System.Collections.Generic;
using Armere.Inventory;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "Find Weapon Routine", menuName = "Game/NPCs/Find Weapon Routine", order = 0)]
public class FindWeaponRoutine : AIContextStateTemplate<Character>
{
	public float maxSearchRange = 15f;
	public LayerMask searchMask = -1;
	public AIContextStateTemplate<Character> foundWeaponRoutine;
	public AIContextStateTemplate<Character> notFoundWeaponRoutine;

	public override AIState StartState(AIMachine c)
	{
		Assert.IsNotNull(context);
		var s = new FindWeapon(c, this, context);
		context = null;
		return s;
	}
}
public class FindWeapon : AIState<FindWeaponRoutine>
{
	readonly Character character;
	public FindWeapon(AIMachine c, FindWeaponRoutine t, Character character) : base(c, t)
	{
		//Find weapons near to this enemy within certain range
		this.character = character;

	}
	public override void Start()
	{
		SearchForItems();
	}
	public void SearchForItems()
	{
		InteractableItem best = null;
		MeleeWeaponItemData bestItem = null;

		foreach (Collider col in Physics.OverlapSphere(c.transform.position, t.maxSearchRange, t.searchMask, QueryTriggerInteraction.Collide))
		{
			if (col.TryGetComponent<InteractableItem>(out var i) &&
				i.item is MeleeWeaponItemData m &&
				(best == null || m.damage > bestItem.damage))
			{
				best = i;
				bestItem = m;
			}
		}
		if (best == null)
		{
			//Not item in range, resort to throwing rocks or something
			Debug.Log($"{c.name} found no weapon");
			machine.ChangeToState(t.notFoundWeaponRoutine?.Target(character));
		}
		else
		{
			//Go and try to pickup the item
			c.StartCoroutine(c.PickupItemRoutine(best, OnItemPickup));
		}
	}
	public void OnItemPickup(bool success)
	{
		//some failures are from full inventory, do not allow items added with magic to create inf loop
		if (success || c.inventory.HasMeleeWeapon)
		{
			c.inventory.selectedMelee = c.inventory.BestMeleeWeapon;
			Spawner.OnDone(c.SetHeldMelee(c.inventory.SelectedMeleeWeapon), _ => Engage());
		}
		else
			SearchForItems(); // Look again until no more items left
	}
	public void Engage()
	{
		machine.ChangeToState(t.foundWeaponRoutine.Target(character));
	}

}