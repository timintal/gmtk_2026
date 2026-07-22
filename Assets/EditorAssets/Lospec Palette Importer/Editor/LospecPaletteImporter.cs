using YesDev.Lospec;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace YesDev.Lospec
{
	public class LospecPaletteImporter : EditorWindow
	{
		const string DefaultAssetDirectory = "Assets/Lospec Palette Importer/Palettes";

		string directoryPath = DefaultAssetDirectory;
		string URL = "https://lospec.com/palette-list/byte-12";
		bool CreateColorPalette = true;
		public bool CreateScriptableObject = true;

		[MenuItem("Tools/ Lospec Palette")]
		public static void ShowWindow()
		{
			GetWindow(typeof(LospecPaletteImporter));
		}

		private void OnGUI()
		{
			GUILayout.Label("Lospec Color Palette Importer", EditorStyles.boldLabel);
			URL = EditorGUILayout.TextField("URL", URL);
			CreateColorPalette = EditorGUILayout.Toggle("Create Color Palette", CreateColorPalette);
			CreateScriptableObject = EditorGUILayout.Toggle("Create Scriptable Object", CreateScriptableObject);

			if (CreateScriptableObject)
			{
				directoryPath = EditorGUILayout.TextField("Asset Folder", directoryPath);
			}

			if (CreateColorPalette)
			{
				EditorGUILayout.LabelField("Unity Presets Folder", GetUnityColorPresetsDirectory());
			}

			if (GUILayout.Button("Create Palette"))
			{
				Execute();
			}
		}

		public void Execute()
		{
			if (!CreateColorPalette && !CreateScriptableObject) return;
			if (string.IsNullOrWhiteSpace(URL)) return;

			string jsonUrl = URL.TrimEnd('/') + ".json";
			string stringJson;

			using (UnityWebRequest request = UnityWebRequest.Get(jsonUrl))
			{
				var operation = request.SendWebRequest();
				while (!operation.isDone)
				{
					// EditorWindow button handler — blocking is acceptable here.
				}

				if (request.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError($"Failed to fetch palette from {jsonUrl}: {request.error}");
					return;
				}

				stringJson = request.downloadHandler.text;
			}

			if (stringJson.Contains("\"error\""))
			{
				Debug.LogError("Lospec returned an error for this palette URL.");
				return;
			}

			PaletteInformation paletteInformation = JsonUtility.FromJson<PaletteInformation>(stringJson);
			if (paletteInformation == null || paletteInformation.colors == null || paletteInformation.colors.Length == 0)
			{
				Debug.LogError("Failed to parse palette data from Lospec.");
				return;
			}

			if (CreateColorPalette)
			{
				GenerateColorPalette(paletteInformation);
			}

			if (CreateScriptableObject)
			{
				GenerateScriptableObject(paletteInformation);
			}
		}

		static string GetUnityColorPresetsDirectory()
		{
			string editorRoot;

			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				editorRoot = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.Personal),
					"Library", "Preferences", "Unity");
			}
			else if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				editorRoot = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"Unity");
			}
			else
			{
				editorRoot = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.Personal),
					".config", "unity3d");
			}

			return Path.Combine(editorRoot, "Editor-5.x", "Presets");
		}

		static string SanitizeFileName(string fileName)
		{
			foreach (char invalidChar in Path.GetInvalidFileNameChars())
			{
				fileName = fileName.Replace(invalidChar, '_');
			}

			return fileName.Trim();
		}

		private void GenerateColorPalette(PaletteInformation paletteInformation)
		{
			string presetsDirectory = GetUnityColorPresetsDirectory();
			if (!Directory.Exists(presetsDirectory))
			{
				Directory.CreateDirectory(presetsDirectory);
			}

			string palettePath = Path.Combine(presetsDirectory, SanitizeFileName(paletteInformation.name) + ".colors");

			StringBuilder newString = new StringBuilder();
			newString.AppendLine("%YAML 1.1");
			newString.AppendLine("%TAG !u! tag:unity3d.com,2011:");
			newString.AppendLine("--- !u!114 &1");
			newString.AppendLine("MonoBehaviour:");
			newString.AppendLine("  m_ObjectHideFlags: 52");
			newString.AppendLine("  m_CorrespondingSourceObject: {fileID: 0}");
			newString.AppendLine("  m_PrefabInstance: {fileID: 0}");
			newString.AppendLine("  m_PrefabAsset: {fileID: 0}");
			newString.AppendLine("  m_GameObject: {fileID: 0}");
			newString.AppendLine("  m_Enabled: 1");
			newString.AppendLine("  m_EditorHideFlags: 0");
			newString.AppendLine("  m_Script: {fileID: 12323, guid: 0000000000000000e000000000000000, type: 0}");
			newString.AppendLine("  m_Name: ");
			newString.AppendLine("  m_EditorClassIdentifier: ");
			newString.AppendLine("  m_Presets:");

			foreach (Color color in paletteInformation.GetColors())
			{
				newString.AppendLine("  - m_Name:");
				newString.AppendLine(
					"    m_Color: {r: " + color.r.ToString(CultureInfo.InvariantCulture)
					+ ", g: " + color.g.ToString(CultureInfo.InvariantCulture)
					+ ", b: " + color.b.ToString(CultureInfo.InvariantCulture)
					+ ", a: 1}");
			}

			File.WriteAllText(palettePath, newString.ToString());
			Debug.Log($"Successfully added Unity color palette '{paletteInformation.name}' to {palettePath}");
		}

		private void GenerateScriptableObject(PaletteInformation paletteInformation)
		{
			string assetDirectory = directoryPath.Replace('\\', '/').TrimEnd('/');
			string assetPath = $"{assetDirectory}/{SanitizeFileName(paletteInformation.name)}.asset";

			if (!Directory.Exists(assetDirectory))
			{
				Directory.CreateDirectory(assetDirectory);
			}

			LospecPaletteObject asset = ScriptableObject.CreateInstance<LospecPaletteObject>();
			asset.SetUp(paletteInformation);

			AssetDatabase.CreateAsset(asset, assetPath);
			EditorUtility.SetDirty(asset);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Debug.Log($"Successfully added ScriptableObject '{paletteInformation.name}' at {assetPath}");
		}
		public string PageUrl(int index)
		{
			return "https://lospec.com/palette-list/load?colorNumberFilterType=any&colorNumber=8&page=" + index + "&tag=&sortingType=default";
		}

		public string PaletteUrl(string pid)
		{
			return "https://lospec.com/palette-list/" + pid;
		}

	}
}

