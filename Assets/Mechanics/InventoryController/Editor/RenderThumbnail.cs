using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.AddressableAssets;

using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

using Armere.Inventory;
[RequireComponent(typeof(Camera))]
public class RenderThumbnail : MonoBehaviour
{
	public PhysicsItemData[] targets;
	public Color key;
	public int size = 256;

	public static float root2 = Mathf.Sqrt(2);

	public Transform holder;

	public T GetComponentAnywhere<T>(GameObject obj) where T : Component
	{
		if (obj.TryGetComponent<T>(out var c))
		{
			return c;
		}
		else
		{
			return obj.GetComponentInChildren<T>();
		}
	}
	string GetPath(PhysicsItemData target)
	{

		string path = AssetDatabase.GetAssetPath(target);
		path = path.Substring(0, path.Length - 6) + "_Thumb.png";
		return path;
	}

	[MyBox.ButtonMethod]
	public void Render()
	{
		Texture2D thumb = new Texture2D(size, size, TextureFormat.ARGB32, true);
		RenderTexture tex = new RenderTexture(size, size, 16);

		Vector3 camForward = transform.forward;
		string[] paths = new string[targets.Length];
		foreach (PhysicsItemData target in targets)
		{

			GameObject copy = Instantiate(target.gameObject.editorAsset, holder);

			//Scale the copy to fit the thumbnail size
			Renderer f = GetComponentAnywhere<Renderer>(copy);
			Bounds bounds = f.bounds;

			transform.position = bounds.center - camForward * 2;


			RenderTexture.active = tex;
			GetComponent<Camera>().orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y) * 0.4f + 0.1f;
			GetComponent<Camera>().targetTexture = tex;
			GetComponent<Camera>().Render();
			GetComponent<Camera>().targetTexture = null;
			DestroyImmediate(copy);





			thumb.ReadPixels(new Rect(0, 0, size, size), 0, 0);
			thumb.Apply();

			Color[] pix = thumb.GetPixels(0, 0, size, size);

			for (int i = 0; i < pix.Length; i++)
			{

				if (pix[i] == key)
				{
					pix[i] = Color.clear;
				}
			}
			thumb.SetPixels(pix);
			thumb.Apply();


			RenderTexture.active = null;
			thumb.alphaIsTransparency = true;

			System.IO.File.WriteAllBytes(GetPath(target), thumb.EncodeToPNG());


		}
		DestroyImmediate(tex);

		AssetDatabase.Refresh();

		//Configure textures
		foreach (PhysicsItemData target in targets)
		{
			string path = GetPath(target);
			TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
			importer.textureType = TextureImporterType.Sprite;
			importer.alphaIsTransparency = true;

			importer.SaveAndReimport();

			//Use this object to manipulate addressables

			var settings = AddressableAssetSettingsDefaultObject.Settings;

			//Make a gameobject an addressable
			string group_name = "Default Local Group";

			AddressableAssetGroup g = settings.FindGroup(group_name);

			var guid = AssetDatabase.AssetPathToGUID(path);

			//This is the function that actually makes the object addressable

			var entry = settings.CreateOrMoveEntry(guid, g);

			entry.labels.Add("thumbnail");

			entry.address = target.name + "_Thumb";

			//You'll need these to run to save the changes!

			settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);




		}
		AssetDatabase.SaveAssets();
		//Link assets
		foreach (PhysicsItemData target in targets)
		{
			string path = GetPath(target);

			target.thumbnail.SetEditorAsset(AssetDatabase.LoadAssetAtPath<Object>(path));
			EditorUtility.SetDirty(target);
		}
	}
}