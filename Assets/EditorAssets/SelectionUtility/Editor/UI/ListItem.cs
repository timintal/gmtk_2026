namespace Xarbrough.SelectionUtility
{
	using UnityEngine;

	internal struct ListItem
	{
		public readonly GameObject GameObject;
		public readonly string Name;

		/// <summary>
		/// Depth relative to the top-most ancestor that is also part of the same list.
		/// Used to render a hierarchy overview via indentation (0 = root, no indent).
		/// </summary>
		public readonly int IndentLevel;

		public ListItem(GameObject gameObject, string name, int indentLevel = 0)
		{
			GameObject = gameObject;
			Name = name;
			IndentLevel = indentLevel;
		}
	}
}