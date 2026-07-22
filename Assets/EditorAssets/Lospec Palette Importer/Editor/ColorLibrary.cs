using System;
using System.Collections.Generic;
using UnityEngine;

namespace YesDev.Lospec
{
	public static class ColorLibrary
	{
		public static List<Color> ConvertColors(string[] stringColors)
		{
			List<Color> colors = new List<Color>();
			foreach (string colorCode in stringColors)
			{
				colors.Add(GetColorFromString(colorCode));
			}
			return colors;
		}

		public static int HexToDec(string hex)
		{
			int dec = Convert.ToInt32(hex, 16);
			return dec;
		}
		public static float HexToNormalized(string hex)
		{
			return HexToDec(hex) / 255f;
		}

		public static Color GetColorFromString(string hexString)
		{
			float red = HexToNormalized(hexString.Substring(0, 2));
			float green = HexToNormalized(hexString.Substring(2, 2));
			float blue = HexToNormalized(hexString.Substring(4, 2));

			return new Color(red, green, blue);
		}
	}
}
