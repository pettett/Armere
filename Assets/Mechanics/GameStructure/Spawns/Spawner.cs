using System.Threading.Tasks;
using UnityEngine;

public abstract class Spawner : MonoBehaviour
{
    public abstract Task<SpawnableBody> Spawn();
}