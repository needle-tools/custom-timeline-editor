using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public class AssetDatabaseProvider
	{
		public void TestSaveOnce(Object rootAsset)
		{ 
			var objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(rootAsset));
			foreach (var obj in objs)
			{
				if (obj is JsonContainer c)
				{
					c.name = "test";
					c.Id = "test";
					c.Content = "321";
					// AssetDatabase.ExtractAsset()
					// Object.DestroyImmediate(obj);
					return; 
				}
			}
			var container = ScriptableObject.CreateInstance<JsonContainer>();
			container.name = "test";
			container.Id = "test";
			container.Content = "123"; 
			AssetDatabase.AddObjectToAsset(container, rootAsset);
			AssetDatabase.SaveAssets(); 
		}
	}
}