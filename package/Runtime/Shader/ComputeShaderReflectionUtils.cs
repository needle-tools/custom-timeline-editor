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
	public class ShaderFieldInfo
	{
		public ComputeShader Shader;
		public string ShaderPath;
		public int KernelIndex;
		public string FieldName;
		public Type? FieldType;
		public string TypeName;

		public ShaderFieldInfo(ComputeShader shader, string shaderPath, int kernelIndex, string fieldName, Type? fieldType, string typeName)
		{
			Shader = shader;
			ShaderPath = shaderPath;
			KernelIndex = kernelIndex;
			FieldName = fieldName;
			FieldType = fieldType;
			TypeName = typeName;
		}
	}
	
	public static class ComputeShaderReflectionUtils
	{
		public static void FindFields(this ComputeShader? shader, List<ShaderFieldInfo> entries)
		{
			if (!shader) return;
			var asset = AssetDatabase.GetAssetPath(shader);
			FindFields(asset, entries);
		}
		
		public static void FindFields(string shaderPath, List<ShaderFieldInfo> entries)
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