using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
	public AnimationController animationController;
	public ParticleSystem system;
	public bool useParticleSystem;
	public AudioClipSet footStepsSet;
	public AudioEventChannelSO onFootstep;
	public Quaternion rotationOffset;
	// Start is called before the first frame update
	void Start()
	{
		animationController.onFootDown += OnFootstep;
	}
	private void OnDestroy()
	{

		animationController.onFootDown -= OnFootstep;
	}

	// Update is called once per frame
	void Update()
	{

	}
	void OnFootstep(int foot)
	{
		Transform f = animationController.anim.GetBoneTransform(foot < 0 ? HumanBodyBones.RightToes : HumanBodyBones.LeftToes);
		system.transform.position = f.position;
		onFootstep.RaiseEvent(footStepsSet, f.position);
		if (useParticleSystem)
		{
			var m = system.main;
			Vector3 rot = (f.rotation * rotationOffset).eulerAngles;

			m.startRotationX = rot.x;
			m.startRotationY = rot.y;
			m.startRotationZ = rot.z;
			system.Emit(1);
		}
	}
}
