using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.UI
{
	public class UINotificationLabel : MonoBehaviour
	{
		public TMPro.TextMeshProUGUI label;
		public StringEventChannelSO notificationEventChannel;
		public float fadeInTime = 0.1f;
		public float persistentTime = 1f;
		public float fadeOutTime = 1f;

		private void OnEnable()
		{
			notificationEventChannel.OnEventRaised += Notify;
		}
		private void OnDisable()
		{
			notificationEventChannel.OnEventRaised -= Notify;
		}
		public void Notify(string text) => StartCoroutine(NotifyRoutine(text));
		IEnumerator NotifyRoutine(string text)
		{
			label.enabled = true;
			label.text = text;

			label.canvasRenderer.SetAlpha(0);
			label.CrossFadeAlpha(1, fadeInTime, true);


			yield return new WaitForSecondsRealtime(persistentTime + fadeInTime);

			label.CrossFadeAlpha(0, fadeOutTime, true);

			yield return new WaitForSecondsRealtime(fadeOutTime);

			label.enabled = false;
		}
	}
}
