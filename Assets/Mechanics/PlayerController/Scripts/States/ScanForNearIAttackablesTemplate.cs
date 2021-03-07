using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Scan for near attackables")]
	public class ScanForNearIAttackablesTemplate : ScanForNearTTemplate<IAttackable>
	{
	}
}