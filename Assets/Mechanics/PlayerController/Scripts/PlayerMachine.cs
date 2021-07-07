using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.PlayerController
{

	public class PlayerMachine : StateMachine<MovementState, PlayerMachine, MovementStateTemplate>
	{
		//set capacity to 1 as it is common for the player to be touching the ground in at least one point
		[NonSerialized] public List<ContactPoint> allCPs = new List<ContactPoint>(1);
		public PlayerController c;

		private void OnDestroy()
		{

			if (currentStates != null)
				foreach (var s in currentStates) s.End();
		}

		public bool StateActive(int i, bool paused = false) => !paused || paused && currentStates[i].updateWhilePaused;

		protected void OnCollisionEnter(Collision col)
		{
			allCPs.Capacity += col.contactCount;
			for (int i = 0; i < col.contactCount; i++)
			{
				allCPs.Add(col.GetContact(i));
			}
		}
		private void OnCollisionStay(Collision col)
		{
			allCPs.Capacity += col.contactCount;
			for (int i = 0; i < col.contactCount; i++)
			{
				allCPs.Add(col.GetContact(i));
			}
		}

		protected override void Update()
		{

			for (int i = 0; i < currentStates.Count; i++)
				if (StateActive(i, c.paused))
					currentStates[i].Update();
		}

		private void LateUpdate()
		{
			for (int i = 0; i < currentStates.Count; i++)
				if (StateActive(i, c.paused))
					currentStates[i].LateUpdate();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (enabled)
				for (int i = 0; i < currentStates.Count; i++)
					if (StateActive(i, c.paused))
						currentStates[i].OnTriggerEnter(other);
		}

		private void OnTriggerExit(Collider other)
		{
			if (enabled)
				for (int i = 0; i < currentStates.Count; i++)
					if (StateActive(i, c.paused))
						currentStates[i].OnTriggerExit(other);
		}

		protected override void FixedUpdate()
		{

			for (int i = 0; i < currentStates.Count; i++)
				if (StateActive(i, c.paused))
					currentStates[i].FixedUpdate();
			allCPs.Clear();
		}
		private void OnAnimatorIK(int layerIndex)
		{
			for (int i = 0; i < currentStates.Count; i++)
				if (StateActive(i, c.paused))
					currentStates[i].OnAnimatorIK(layerIndex);
		}


		public void ChangeToStateTimed(MovementStateTemplate timedState, float time, MovementStateTemplate returnedState = null) =>
			StartCoroutine(ChangeToStateTimedRoutine(timedState, time, returnedState));

		IEnumerator ChangeToStateTimedRoutine(MovementStateTemplate timedState, float time, MovementStateTemplate returnedState = null)
		{
			ChangeToState(timedState);
			yield return new WaitForSeconds(time);
			//Returned state or default state if it is null
			ChangeToState(returnedState ?? defaultState);
		}


		public override MovementState ChangeToState(MovementStateTemplate t)
		{

			var x = base.ChangeToState(t);

			if (c.entry != null)
			{
				//Show all the currentley active states
				//update f3 information
				c.entry.Clear();
				c.entry.Append("Current States: ");

				foreach (var s in currentStates)
				{
					c.entry.Append(s.StateName);
					c.entry.Append(',');
				}
			}


			return x;
		}


		private void OnDrawGizmos()
		{
			if (currentStates != null)
				for (int i = 0; i < currentStates.Count; i++)
					if (StateActive(i))
						currentStates[i].OnDrawGizmos();
		}

	}
}