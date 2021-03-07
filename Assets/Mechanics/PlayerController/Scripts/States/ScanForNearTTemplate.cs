namespace Armere.PlayerController
{

	public class ScanForNearTTemplate<T> : MovementStateTemplate where T : IScanable
	{
		public override MovementState StartState(PlayerController c)
		{
			return new ScanForNearT<T>(c, this);
		}
	}
}