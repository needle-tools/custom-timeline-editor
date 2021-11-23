using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[CreateAssetMenu(menuName = "Needle/Timeline-Clip")]
	public class CodeControlAssetData : ScriptableObject
	{
		public string Id;
		public List<JsonContainer> ClipData = new List<JsonContainer>();

		private void OnValidate()
		{
#if UNITY_EDITOR
			if (string.IsNullOrEmpty(Id))
			{
				if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(GetInstanceID(), out var guid, out long id))
					Id = $"{guid}@{id}"; 
			}  
#endif
		}
	}
}