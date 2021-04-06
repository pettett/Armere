using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
[RequireComponent(typeof(Rig))]
public class BowRig : MonoBehaviour
{
	public Rig rig;
	public Transform bowHolder;
	public GameObject prefab;

	public Transform bowString;

	public Transform lookAtTarget;

	Bow bow;
	public float stringRest;
	public float stringTight;
	public float heightRest;
	public float heightTight;

	[Range(0, 1)]
	public float bowPull;

	[MyBox.ButtonMethod]
	public void Spawn()
	{

		GameObject go = Instantiate(prefab, bowHolder);
		AddToRig(go);
		bow.InitBow(null);
	}
	public void AddToRig(GameObject go)
	{
		rig.weight = 1;
		enabled = true;
		go.transform.SetParent(bowHolder, false);
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		bow = go.GetComponent<Bow>();
	}
	public void ClearRig()
	{
		rig.weight = 0;
		enabled = false;
		bow = null;
	}
	private void Update()
	{
		Vector3 pos = bowString.localPosition;
		pos.z = Mathf.Lerp(stringRest, stringTight, bowPull);
		bowString.localPosition = pos;
		bow.arrowAnchor.position = bowString.position;

		pos = bow.bendChainTop.localPosition;

		pos.y = Mathf.Lerp(heightRest, heightTight, bowPull);

		bow.bendChainTop.localPosition = pos;
		pos.y = -pos.y;

		bow.bendChainBottom.localPosition = pos;
	}
}
