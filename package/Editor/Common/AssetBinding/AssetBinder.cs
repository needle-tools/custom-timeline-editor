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
				var binding = field.GetCustomAttribute<BindAsset>();
				binding.Apply(field);  
			}
		}
	}
}