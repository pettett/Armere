using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Armere.UI
{
	public class AutoDraggerUI : MonoBehaviour
	{
		public GameObject draggerPrefab;
		public void AutoDrag(Sprite sprite, Vector2 start, Vector2 end, float speed)
		{
			GameObject dragger = Instantiate(draggerPrefab, transform);
			dragger.GetComponent<Image>().sprite = sprite;
			dragger.transform.position = start;
			LeanTween.move(dragger, end, Vector2.Distance(start, end) / speed).setOnComplete(() =>
			{
				Destroy(dragger);
			}).setIgnoreTimeScale(true);
		}
	}
}
