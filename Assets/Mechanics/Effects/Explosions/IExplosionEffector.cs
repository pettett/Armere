using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExplosionEffector 
{
	void OnExplosion(Vector3 source, float radius, float force);
}

