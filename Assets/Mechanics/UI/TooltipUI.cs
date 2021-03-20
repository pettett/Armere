using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

namespace Armere.UI
{

	public class TooltipUI : MonoBehaviour
	{
		public static TooltipUI current;

		public TMP_Text title;
		public TMP_Text description;
		public Image thumbnail;

		private void Awake()
		{
			current = this;
			gameObject.SetActive(false);
		}

		public void BeginCursorTooltip(string title, string description)
		{
			this.title.SetText(title);
			this.description.SetText(description);

			SetActive(true, true, false, true);
		}
		public void OnCursorEnterItemUI(Sprite thumbnail)
		{
			this.thumbnail.sprite = thumbnail;


			SetActive(false, false, true, true);
		}
		void SetActive(bool title, bool descition, bool thumbnail, bool gameObject)
		{
			this.title.gameObject.SetActive(title);
			this.description.gameObject.SetActive(descition);
			this.thumbnail.gameObject.SetActive(thumbnail);

			this.gameObject.SetActive(gameObject);
		}


		public void EndCursorTooltip()
		{
			SetActive(false, false, false, false);
		}

		private void OnGUI()
		{
			SetPostion(Event.current.mousePosition);
		}

		public void SetPostion(Vector2 pos)
		{

			transform.position = (pos + new Vector2(20, 10)) * new Vector2(1, -1) + new Vector2(0, Screen.height);
		}
	}
}