using UnityEngine;
public abstract class GlobalSO<T0> : ScriptableObject
{
	public T0 value;
}

[CreateAssetMenu(fileName = "Vector3 Global", menuName = "Globals/Vector3 Global")]
public class GlobalVector3SO : GlobalSO<Vector3>
{

}