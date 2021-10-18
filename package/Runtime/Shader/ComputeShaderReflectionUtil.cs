﻿#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TreeEditor;
using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	[Serializable]
	public class ShaderFileInfo
	{
		public string Path;
		public List<ComputeShaderFieldInfo> Fields = new List<ComputeShaderFieldInfo>();
		public List<ComputeShaderStructInfo> Structs = new List<ComputeShaderStructInfo>();

		public override string ToString()
		{
			return Path + "\n\nStructs:\n" + string.Join("\n", Structs.Select(s => s.ToString())) + "\n\nFields:\n" +
			       string.Join("\n", Fields.Select(f => f.ToString()));
		}
	}

	[Serializable]
	public class ComputeShaderInfo : ShaderFileInfo
	{
		public ComputeShader? Shader;
		public List<ComputeShaderKernelInfo> Kernels = new List<ComputeShaderKernelInfo>();

		// TODO: implement keywords and includes
		public List<string>? Keywords;
		public List<string>? Includes;

		public override string ToString()
		{
			return "Kernels:\n" + string.Join("\n", Kernels.Select(s => s.ToString())) + "\n\n" + base.ToString();
		}
	}

	[Serializable]
	public class ComputeShaderKernelInfo
	{
		public string Name;
		public int Index;
		public Vector3Int Threads;

		public override string ToString()
		{
			return Name + ", " + Index + ", " + Threads;
		}
	}

	[Serializable]
	public class ComputeShaderFieldInfo
	{
		// TODO: currently we only check in kernel methods if the field is used
		public List<ComputeShaderKernelInfo>? Kernels;
		public string FilePath;
		public string FieldName;
		public Type? FieldType;
		public string TypeName;
		public int Stride;
		public bool RandomWrite;
		public string? GenericTypeName;
		/// <summary>
		/// If the type is a known struct
		/// </summary>
		public ComputeShaderStructInfo? StructInfo;

		public override string ToString()
		{
			var res = "Name=" + FieldName + ", Type=" + FieldType?.FullName + ", Stride= " + Stride + " bytes, RandomWrite=" + RandomWrite;
			if (GenericTypeName != null) res += ", GenericType=" + GenericTypeName;
			if(Kernels != null) res += ", Used in Kernels: " + string.Join(", ", Kernels.Select(k => k.Name));
			return res;
		}
	}

	[Serializable]
	public class ComputeShaderStructInfo
	{
		public string Name;
		public string FileName;
		public List<ComputeShaderFieldInfo> Fields = new List<ComputeShaderFieldInfo>();

		public int CalcStride()
		{
			var sum = 0;
			foreach (var f in Fields)
			{
				if (f.Stride < 0) return -1;
				sum += f.Stride;
			}
			return sum;
		}

		public override string ToString()
		{
			return Name + " - " + CalcStride() + " bytes\n" + string.Join("\n", Fields.Select(f => f.ToString()));
		}
	}

	public static class ComputeShaderReflectionUtil
	{
		public static bool TryParse(this ComputeShader? shader, out ComputeShaderInfo? shaderInfo)
		{
			shaderInfo = null;
			if (!shader) return false;
			shaderInfo = new ComputeShaderInfo();
			var asset = AssetDatabase.GetAssetPath(shader);
			return TryParse(asset, shaderInfo);
		}

		public static bool TryParse(string shaderPath, ComputeShaderInfo shaderInfo)
		{
			if (!shaderPath.EndsWith(".compute") && !shaderPath.EndsWith(".cginc")) return false;
			shaderInfo.Path = shaderPath;

			var txt = File.ReadLines(shaderPath).Where(l => !string.IsNullOrWhiteSpace(l));
			var inStruct = false;
			var blockLevel = 0;
			var currentStructInfo = default(ComputeShaderStructInfo);
			var inKernelFunction = false;
			var currentKernelMethod = default(ComputeShaderKernelInfo);
			Vector3Int? lastKernelThreadsAttributeFound = null;
			foreach (var _line in txt)
			{
				var line = _line.Trim();

				if (line.StartsWith("#"))
				{
					var kernelMatch = kernelRegex.Match(line);
					if (kernelMatch.Success)
					{
						var kernel = new ComputeShaderKernelInfo();
						kernel.Name = kernelMatch.Groups["kernel_name"].Value;
						shaderInfo.Kernels.Add(kernel);
					}
				}
				else if (line.StartsWith("[numthreads("))
				{
					var kernelThreads = kernelThreadsRegex.Match(line);
					if (kernelThreads.Success)
					{
						var g = kernelThreads.Groups;
						lastKernelThreadsAttributeFound = new Vector3Int(
							int.Parse(g[1].Value),
							int.Parse(g[2].Value),
							int.Parse(g[3].Value)
						);
					}
				}
				else if (line.Contains("}"))
				{
					blockLevel -= 1;
					if (blockLevel == 0)
					{
						inStruct = false;
						inKernelFunction = false;
					}
				}
				else if (line.Contains("{"))
				{
					blockLevel += 1;
				}

				if (line.StartsWith("struct"))
				{
					inStruct = true;
					currentStructInfo = new ComputeShaderStructInfo();
					currentStructInfo.Name = line.Trim().Substring("struct".Length).Trim(' ', '{', '}');
					currentStructInfo.FileName = shaderPath;
					shaderInfo.Structs.Add(currentStructInfo);
				}
				else if (line.StartsWith("void"))
				{
					// search for kernel
					var kernelLine = line.Replace("void", "");
					var paramStartIndex = kernelLine.IndexOf('(');
					if (paramStartIndex > 0)
					{
						var methodName = kernelLine.Substring(0, paramStartIndex).Trim();
						var kernel = shaderInfo.Kernels.FirstOrDefault(k => methodName.EndsWith(k.Name));
						if (kernel != null)
						{
							if (lastKernelThreadsAttributeFound != null)
								kernel.Threads = lastKernelThreadsAttributeFound.Value;
							lastKernelThreadsAttributeFound = null;
							inKernelFunction = true;
							currentKernelMethod = kernel;
						}
					}
				}
				else if (blockLevel > 0)
				{
					// find out which fields are actually used in which kernels
					if (inKernelFunction)
					{
						if (currentKernelMethod == null) throw new Exception("Current Kernel method is unknown");
						foreach (var field in shaderInfo.Fields)
						{
							foreach (var i in line.AllIndexesOf(field.FieldName))
							{
								var charBeforeIsOk = i == 0 || allowedSurroundingVariableName.Contains(line[i - 1]);
								if (!charBeforeIsOk) continue;
								var charAfterIsOk = i == line.Length - 1 || allowedSurroundingVariableName.Contains(line[i + field.FieldName.Length]);
								if (!charAfterIsOk) continue;
 								field.Kernels ??= new List<ComputeShaderKernelInfo>();
								if (!field.Kernels.Contains(currentKernelMethod))
									field.Kernels.Add(currentKernelMethod);
								break;
							}
						}
					}
				}
				
				if(blockLevel == 0 || inStruct)
				{
					var fieldMatch = fieldRegex.Match(line.Trim());
					if (fieldMatch.Success)
					{
						var fieldGroup = fieldMatch.Groups;
						var type = fieldGroup["field_type"].Value;
						var generics = fieldGroup["generic_type"].Value;
						var names = fieldMatch.Groups["field_names"].Value;
						foreach (var _name in names.Split(','))
						{
							var name = _name.Trim();
							var field = new ComputeShaderFieldInfo();
							field.FieldName = name;
							field.FilePath = shaderPath;
							field.SetType(type);
							if(!string.IsNullOrEmpty(generics))
								field.GenericTypeName = generics;
							field.Stride = TryGetStride(field.TypeName) ?? TryGetStride(generics) ?? TryFindStrideFromDeclaredStruct() ?? -1;

							int? TryFindStrideFromDeclaredStruct()
							{
								foreach (var str in shaderInfo.Structs)
								{
									if (str.Name == generics)
										return str.CalcStride();
								}
								return null;
							}
							
							if (inStruct)
							{
								currentStructInfo!.Fields.Add(field);
							}
							else
							{
								shaderInfo.Fields.Add(field);
							}
						}
					}
				}
			}
			return true;
		}

		private static readonly char[] allowedSurroundingVariableName = new[] { ' ', '+', '-', '*', '/', '=', '|', ';' };
		
		// https://regex101.com/r/SBWf77/2
		private static readonly Regex fieldRegex = new Regex("((?<field_type>.+?)(<(?<generic_type>.+?)>)?) (?<field_names>.+);",
			RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

		private static readonly Regex kernelRegex = new Regex("#pragma kernel (?<kernel_name>.+)", RegexOptions.Compiled);
		private static readonly Regex kernelThreadsRegex = new Regex(@"\[numthreads\((\d+),(\d+),(\d+)\)\]", RegexOptions.Compiled);

		private static void SetType(this ComputeShaderFieldInfo field, string typeName)
		{
			field.TypeName = typeName;
			switch (typeName)
			{
				case "float":
					field.FieldType = typeof(float);
					break;
				case "float2":
					field.FieldType = typeof(Vector2);
					break;
				case "float3":
					field.FieldType = typeof(Vector3);
					break;
				case "float4":
					field.FieldType = typeof(Vector4);
					break;
				case "int":
					field.FieldType = typeof(int);
					break;
				case "int2":
					field.FieldType = typeof(Vector2Int);
					break;
				case "int3":
					field.FieldType = typeof(Vector3Int);
					break;
				case "int4":
					field.FieldType = typeof(Vector3Int);
					break;
				case "uint":
					field.FieldType = typeof(uint);
					break;
				case "long":
					field.FieldType = typeof(double);
					break;

				case "float4x4":
					field.FieldType = typeof(Matrix4x4);
					break;

				case "StructuredBuffer":
					field.FieldType = typeof(ComputeBuffer);
					break;
				case "RWStructuredBuffer":
					field.FieldType = typeof(ComputeBuffer);
					field.RandomWrite = true;
					break;
				case "Texture2D":
					field.FieldType = typeof(Texture2D);
					break;
				case "RWTexture2D":
					field.FieldType = typeof(Texture2D);
					field.RandomWrite = true;
					break;
			}
		}

		private static int? TryGetStride(string typeName)
		{
			switch (typeName)
			{
				case "bool":
					return sizeof(bool);
				case "long":
					return sizeof(double);
				case "float":
					return sizeof(float);
				case "float2":
					return sizeof(float) * 2;
				case "float3":
					return sizeof(float) * 3;
				case "float4":
					return sizeof(float) * 4;
				case "int":
					return sizeof(int);
				case "int2":
					return sizeof(int) * 2;
				case "int3":
					return sizeof(int) * 3;
				case "int4":
					return sizeof(int) * 4;
				case "uint":
					return sizeof(uint);
				case "uint2":
					return sizeof(uint) * 2;
				case "uint3":
					return sizeof(uint) * 3;
				case "uint4":
					return sizeof(uint) * 4;
				case "float4x4":
					return sizeof(float) * 4 * 4;
			}
			return null;
		}
		
		private static IEnumerable<int> AllIndexesOf(this string str, string search)
		{
			int minIndex = str.IndexOf(search, StringComparison.Ordinal);
			while (minIndex != -1)
			{
				yield return minIndex;
				minIndex = str.IndexOf(search, minIndex + search.Length, StringComparison.Ordinal);
			}
		}
	}
}