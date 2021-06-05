using UnityEngine;
namespace Armere.PlayerController
{
	public static class PlayerInputUtility
	{
		public static Vector3 WorldSpaceFlatInput(PlayerController c) => WorldSpaceFlatInput(c, c.WorldUp).normalized;

		static Vector3 WorldSpaceFlatInput(PlayerController c, Vector3 up)
		{
			return Vector3.ProjectOnPlane(GameCameras.s.TransformInput(c.inputReader.horizontalMovement, up), up);
		}
		public static Vector3 WorldSpaceFullInput(PlayerController c)
		{
			return (WorldSpaceFlatInput(c) + c.WorldUp * c.inputReader.verticalMovement).normalized;
		}
	}
}