using System.Collections.Generic;
using UnityEngine;

namespace YesDev.Lospec
{
	public class PaletteInformation
	{
		public string name = "No Name";
		public string author = "No Author";
		public string[] colors = new string[0];

		public List<Color> GetColors()
		{
			return ColorLibrary.ConvertColors(colors);
		}
	}
}
