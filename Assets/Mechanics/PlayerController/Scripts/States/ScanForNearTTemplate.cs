namespace Armere.PlayerController
{

	public class ScanForNearTTemplate<T> : MovementStateTemplate where T : IScanable
	{
		public override MovementState StartState(PlayerMachine c)
		{
			return new ScanForNearT<T>(c, this);
		}
	}
}