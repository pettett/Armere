using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Save Data")]
	public class PlayerSaveData : SaveableSO
	{
		public PlayerController c;
		public Vector3 position;
		public Quaternion rotation;
		public EquipmentSet<bool> sheathedItems;
		public MovementStateTemplate startingState = null;

		public readonly int[] armourSelections = new int[3] { -1, -1, -1 };
		public readonly Dictionary<ItemType, int> itemSelections = new Dictionary<ItemType, int>(new ItemTypeEqualityComparer()){
			{ItemType.Melee,-1},
			{ItemType.Bow,-1},
			{ItemType.Ammo,-1},
			{ItemType.SideArm,-1},
		};
		public static MovementStateTemplate SymbolToType(char symbol)
		{
			//Search assembly for the symbol by creating an instance of every class and comparing - slow
			foreach (var t in Resources.LoadAll<MovementStateTemplate>("PlayerController"))
				//Create an instance of the type and check it's symbol
				if (t.stateSymbol == symbol)
					return t;
			throw new ArgumentException("Symbol not mapped to state");
		}


		public override void LoadBin(in GameDataReader reader)
		{
			position = reader.ReadVector3();

			rotation = reader.ReadQuaternion();

			armourSelections[0] = reader.ReadInt();
			armourSelections[1] = reader.ReadInt();
			armourSelections[2] = reader.ReadInt();

			itemSelections[ItemType.Melee] = reader.ReadInt();
			itemSelections[ItemType.SideArm] = reader.ReadInt();
			itemSelections[ItemType.Bow] = reader.ReadInt();
			itemSelections[ItemType.Ammo] = reader.ReadInt();

			sheathedItems = new EquipmentSet<bool>(reader.ReadBool(), reader.ReadBool(), reader.ReadBool());

			startingState = SymbolToType(reader.ReadChar());
		}

		public override void LoadBlank()
		{
		}

		public override void SaveBin(in GameDataWriter writer)
		{
			writer.WritePrimitive(c.transform.position);
			writer.WritePrimitive(c.transform.rotation);

			writer.WritePrimitive(armourSelections[0]);
			writer.WritePrimitive(armourSelections[1]);
			writer.WritePrimitive(armourSelections[2]);

			writer.WritePrimitive(itemSelections[ItemType.Melee]);
			writer.WritePrimitive(itemSelections[ItemType.SideArm]);
			writer.WritePrimitive(itemSelections[ItemType.Bow]);
			writer.WritePrimitive(itemSelections[ItemType.Ammo]);

			writer.WritePrimitive(c.weaponGraphics.holdables.melee.sheathed);
			writer.WritePrimitive(c.weaponGraphics.holdables.sidearm.sheathed);
			writer.WritePrimitive(c.weaponGraphics.holdables.bow.sheathed);

			//Save the current state
			writer.WritePrimitive(c.currentState.stateSymbol);

		}
	}
}
