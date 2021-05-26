using UnityEngine;
namespace Armere.PlayerController
{
	public static class PlayerInputUtility
	{
		public static Vector3 WorldSpaceFlatInput(PlayerController c)
		{
			Vector3 playerDirection = GameCameras.s.TransformInput(c.inputReader.horizontalMovement);
			playerDirection.y = 0;
			return playerDirection.normalized;
		}
		public static Vector3 WorldSpaceFlatInput(PlayerController c, Vector3 up)
		{
			return Vector3.ProjectOnPlane(GameCameras.s.TransformInput(c.inputReader.horizontalMovement, up), up);
		}
		public static Vector3 WorldSpaceFullInput(PlayerController c)
		{
			Vector3 playerDirection = GameCameras.s.TransformInput(c.inputReader.horizontalMovement);
			playerDirection.y = c.inputReader.verticalMovement;
			return playerDirection.normalized;
		}
	}
}