using System;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Needle.Timeline.AssetBinding
{
	[AttributeUsage(AttributeTargets.Field)]
	public class BindAsset : Attribute
	{
		public static readonly Object CouldNotLoadMarker = new Object();
		
		public readonly string Guid;

		public BindAsset(string guid)
		{
			this.Guid = guid;
		}

		public Object Apply(FieldInfo field, object instance = null)
		{
			if (!field.IsStatic && instance == null) return null;
			if (string.IsNullOrEmpty(Guid)) return null;
			var value = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(Guid), field.FieldType);
			field.SetValue(null, value);
			if (!value) return CouldNotLoadMarker;
			return value;
		}
	}
}