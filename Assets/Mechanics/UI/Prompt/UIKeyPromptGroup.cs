using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;
using Malee.List;

[RequireComponent(typeof(LayoutGroup))]
public class UIKeyPromptGroup : MonoBehaviour
{
	[System.Serializable]
	public struct KeyPrompt
	{
		public string name;
		public string action;

		public KeyPrompt(string name, string action)
		{
			this.name = name;
			this.action = action;
		}

		public static implicit operator KeyPrompt((string name, string action) s) => new KeyPrompt(s.name, s.action);

	}
	[System.Serializable]
	public struct KeyBindSprite
	{
		public string startsWith;
		public Sprite sprite;
		public bool includeLabel;
	}
	public float preferredWidth = 100f;
	public float preferredHeight = 100f;
	public float fontSize = 12f;
	public float buttonGapSize = 10f;
	public float gapSize = 80f;
	public TMP_FontAsset fontAsset;
	public Material maskingTextMaterial;
	public Material maskedSpriteMaterial;

	[System.Serializable]
	public class KeyBindSpritesArray : ReorderableArray<KeyBindSprite> { }
	[Reorderable(paginate = false)]
	public KeyBindSpritesArray keybindSpritesOrder;
	public static UIKeyPromptGroup singleton;



	private void Start()
	{
		//ShowPrompts(player, "Ground Action Map", starts);
		//player = PlayerInput.all[0];
	}
	private void Awake()
	{
		singleton = this;
	}
	public void RemovePrompts()
	{
		foreach (Transform child in transform) Destroy(child.gameObject);
	}

	public void ShowPrompts(InputReader input, string map, params KeyPrompt[] prompts)
	{



		for (int i = 0; i < prompts.Length; i++)
		{


			//Keybind text


			var bindText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
			var bindT = bindText.GetComponent<TextMeshProUGUI>();
			bindT.font = fontAsset;
			bindT.fontSize = fontSize;
			bindT.fontMaterial = maskingTextMaterial;
			bindT.text = input.GetBindingDisplayString(map, prompts[i].action);

			bindT.alignment = TextAlignmentOptions.Right;

			bindText.transform.SetParent(transform, false);

			// ExpandRectTransform(bindText.transform as RectTransform, Vector2.zero, new Vector2(0.5f, 1f));

			var bindImage = new GameObject("Sprite", typeof(RectTransform), typeof(Image));
			bindImage.transform.SetParent(bindText.transform, false);

			ExpandRectTransform((RectTransform)bindImage.transform, Vector2.zero, Vector2.one);

			bindImage.GetComponent<Image>().material = maskedSpriteMaterial;

			//Name text
			var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
			var t = text.GetComponent<TextMeshProUGUI>();
			t.fontSize = fontSize;
			t.font = fontAsset;
			t.text = prompts[i].name;
			t.alignment = TextAlignmentOptions.Left;
			text.transform.SetParent(transform, false);


			t.margin = new Vector4(buttonGapSize, 0, i != prompts.Length - 1 ? gapSize : 0, 0);



			//  ExpandRectTransform(text.transform as RectTransform, new Vector2(0.5f, 0), Vector2.one);

		}
	}

	public static void ExpandRectTransform(RectTransform transform, Vector2 anchorMin, Vector2 anchorMax)
	{
		transform.anchorMin = anchorMin;
		transform.anchorMax = anchorMax;
		transform.anchoredPosition = Vector3.zero;
		transform.sizeDelta = Vector3.zero;
	}

}
