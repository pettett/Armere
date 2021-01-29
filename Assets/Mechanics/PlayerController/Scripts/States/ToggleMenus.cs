using UnityEngine;
using System;
using UnityEngine.InputSystem;
namespace Armere.PlayerController
{
	//allow the player to access the menu system if tab is pressed
	[System.Serializable]
	public class ToggleMenus : MovementState
	{
		public override string StateName => "Enter Menus";
		public override char StateSymbol => 't';

		[NonSerialized] bool inMenus;
		[NonSerialized] bool inConsole;


		public override void Start()
		{
			c.inputReader.tabMenuEvent += OnTabMenu;
			c.inputReader.consoleEvent += OnConsole;
			c.inputReader.openInventoryEvent += OnOpenInventory;
			c.inputReader.openQuestsEvent += OnOpenQuests;
			c.inputReader.openMapEvent += OnOpenMap;
		}
		public override void End()
		{
			c.inputReader.tabMenuEvent -= OnTabMenu;
			c.inputReader.consoleEvent -= OnConsole;
		}


		public void OnTabMenu(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)
			{
				ToggleTabMenu();
			}
		}

		public void OnOpenInventory(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)
			{
				c.setTabMenuPanelEventChannel.RaiseEvent(2);
				if (!inMenus)
					ToggleTabMenu();
			}
		}
		public void OnOpenQuests(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)
			{
				c.setTabMenuPanelEventChannel.RaiseEvent(1);
				if (!inMenus)
					ToggleTabMenu();
			}
		}
		public void OnOpenMap(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)
			{
				c.setTabMenuPanelEventChannel.RaiseEvent(3);
				if (!inMenus)
					ToggleTabMenu();
			}
		}


		public void ToggleTabMenu()
		{
			//Only enter if the game is not paused
			if (inMenus) inMenus = false;
			else if (!c.paused) inMenus = true;
			else return;

			UpdateMenus();
		}

		public void OnConsole(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)
			{
				inConsole = !inConsole;
				UpdateConsole();
			}
		}

		void UpdateConsole()
		{
			if (inConsole)
			{
				SetPaused(true);
				Console.Enable(() =>
				{
					inConsole = false;
					UpdateConsole();
				});
			}
			else
			{
				Console.Disable();
				SetPaused(false);
			}
		}



		void UpdateMenus()
		{
			c.setTabMenuEventChannel.RaiseEvent(inMenus);
			SetPaused(inMenus);
		}

		void SetPaused(bool p)
		{
			if (p)
			{
				c.PauseControl();
			}
			else
			{
				c.ResumeControl();
			}
		}
	}

}