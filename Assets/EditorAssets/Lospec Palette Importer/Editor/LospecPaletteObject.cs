using System.Collections.Generic;
using UnityEngine;

namespace YesDev.Lospec
{
    [CreateAssetMenu(fileName = "Lospec Color Palette",menuName ="Lospec/Color")]
	public class LospecPaletteObject : ScriptableObject
	{
		public string Author;
		public List<Color> colors = new List<Color>();


		public void SetUp(PaletteInformation paletteInformation)
		{
			Author = paletteInformation.author;
			name = paletteInformation.name;
			colors = paletteInformation.GetColors();
		}
	
	}
}
