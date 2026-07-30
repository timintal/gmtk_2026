namespace Xarbrough.SelectionUtility
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	internal static class Hierarchy
	{
		public static List<GameObject> GetDisplayHierarchy(GameObject leaf)
		{
			var elements = GetParents(leaf).ToList();
			elements.Add(leaf);
			return CollapseList(elements, maxCount: 32);
		}

		public static IEnumerable<GameObject> GetParents(GameObject go)
		{
			return GetParentsLeafToRoot(go).Reverse();

			static IEnumerable<GameObject> GetParentsLeafToRoot(GameObject go)
			{
				while (go.transform.parent != null)
				{
					go = go.transform.parent.gameObject;
					yield return go;
				}
			}
		}
		
		public static List<T> CollapseList<T>(List<T> list, int maxCount, T omittedValue = default)
		{
			if (maxCount < 3)
				throw new ArgumentException();

			int itemsToRemove = list.Count - maxCount;

			if (itemsToRemove < 1)
				return list;

			int startIndex = (list.Count - itemsToRemove + 1) / 2 - 1;
			list.RemoveRange(startIndex, itemsToRemove + 1);
			list.Insert(startIndex, omittedValue);

			Debug.Assert(list.Count <= maxCount);
			return list;
		}

		/// <summary>
		/// Arranges a flat set of GameObjects into a hierarchy overview: each object is placed
		/// directly above its descendants (pre-order), and tagged with an indent level relative
		/// to the top-most ancestor that is also part of the set. Objects that are not related
		/// to each other stay as separate roots with no indentation.
		/// </summary>
		/// <remarks>
		/// The incoming order of the objects is preserved for roots and for siblings under the
		/// same parent, so any upstream sorting (draw order or transform hierarchy) still applies
		/// within each level.
		/// </remarks>
		public static List<ListItem> ArrangeAsOverview(IReadOnlyList<GameObject> gameObjects)
		{
			var result = new List<ListItem>(gameObjects.Count);
			if (gameObjects.Count == 0)
				return result;

			var set = new HashSet<GameObject>(gameObjects);
			var childrenByParent = new Dictionary<GameObject, List<GameObject>>();
			var roots = new List<GameObject>();

			foreach (GameObject go in gameObjects)
			{
				if (go == null)
					continue;

				GameObject parent = GetNearestAncestorInSet(go, set);
				if (parent == null)
				{
					roots.Add(go);
				}
				else
				{
					if (!childrenByParent.TryGetValue(parent, out List<GameObject> children))
					{
						children = new List<GameObject>();
						childrenByParent[parent] = children;
					}
					children.Add(go);
				}
			}

			foreach (GameObject root in roots)
				AddWithChildren(root, 0);

			return result;

			void AddWithChildren(GameObject go, int indent)
			{
				result.Add(new ListItem(go, go.name, indent));

				if (childrenByParent.TryGetValue(go, out List<GameObject> children))
				{
					foreach (GameObject child in children)
						AddWithChildren(child, indent + 1);
				}
			}
		}

		private static GameObject GetNearestAncestorInSet(GameObject go, HashSet<GameObject> set)
		{
			Transform current = go.transform.parent;
			while (current != null)
			{
				if (set.Contains(current.gameObject))
					return current.gameObject;

				current = current.parent;
			}

			return null;
		}

		public static void Sort(List<GameObject> list)
		{
			list.Sort((x, y) =>
			{
				int xLevel = GetHierarchyLevel(x);
				int yLevel = GetHierarchyLevel(y);
				
				if (xLevel == yLevel)
				{
					return x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex());
				}
				
				return xLevel.CompareTo(yLevel);
			});
		}
		
		private static int GetHierarchyLevel(GameObject go)
		{
			int level = 0;
			Transform current = go.transform;
			while (current.parent != null)
			{
				level++;
				current = current.parent;
			}

			return level;
		}
	}
}