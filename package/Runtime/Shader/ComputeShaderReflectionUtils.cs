#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[Serializable]
	public class ComputeShaderFieldInfo
	{
		public string FilePath;
		public List<int>? Kernels;
		public string FieldName;
		public Type? FieldType;
		public string TypeName;
		public int Stride;
	}
	
	public static class ComputeShaderReflectionUtils
	{
		public static void FindFields(this ComputeShader? shader, List<ComputeShaderFieldInfo> fields)
		{
			if (!shader) return;
			var asset = AssetDatabase.GetAssetPath(shader);
			FindFields(asset, fields);
		}
		
		public static void FindFields(string shaderPath, List<ComputeShaderFieldInfo> fields)
		{
			if (!shaderPath.EndsWith(".compute") && !shaderPath.EndsWith(".cginc")) return;
			var txt = File.ReadLines(shaderPath);
			foreach (var line in txt)
			{
				Debug.Log(line);
				var fieldMatch = fieldRegex.Match(line.Trim());
				if (fieldMatch.Success)
				{
					var fieldGroup = fieldMatch.Groups;
					var type = fieldGroup["field_type"];
					var generics = fieldGroup["generic_type"];
					var names = fieldMatch.Groups["field_names"];
					Debug.Log("<b>" + type.Value + " :: " + names + "</b> " + generics.Value);
				}
			}
		}
		
		// https://regex101.com/r/SBWf77/2
		internal static readonly Regex fieldRegex = new Regex("((?<field_type>.+?<?(<(?<generic_type>.+?)>)?)) (?<field_names>.+);", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
	}
}