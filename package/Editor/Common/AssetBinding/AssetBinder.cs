using System.Reflection;
using UnityEditor;

namespace Needle.Timeline.AssetBinding
{
	[InitializeOnLoad]
	internal static class AssetBinder
	{
		static AssetBinder()
		{
			var fields = TypeCache.GetFieldsWithAttribute<BindAsset>();
			foreach (var field in fields)
			{
				if (field.IsStatic == false) continue; 
				var binding = field.GetCustomAttribute<BindAsset>();
				if (string.IsNullOrEmpty(binding.Guid)) continue;
				var value = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(binding.Guid), field.FieldType);
				field.SetValue(null, value);
			}
		}
	}
}