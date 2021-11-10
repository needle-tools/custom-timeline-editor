using System;
using System.ComponentModel;
using System.Reflection;
using UnityEditor;

namespace Needle.Timeline.AssetBinding
{
	[AttributeUsage(AttributeTargets.Field)]
	public class BindAsset : Attribute
	{
		public readonly string Guid;

		public BindAsset(string guid)
		{
			this.Guid = guid;
		}

		public void Apply(FieldInfo field, object instance = null)
		{
			if (!field.IsStatic && instance == null) return; 
			if (string.IsNullOrEmpty(Guid)) return;
			var value = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(Guid), field.FieldType);
			field.SetValue(null, value);
		}
	}
}