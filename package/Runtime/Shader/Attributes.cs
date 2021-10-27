#nullable enable

using System;
using System.CodeDom;
using System.Threading;
using Needle.Timeline.ResourceProviders;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Needle.Timeline
{
	[AttributeUsage(AttributeTargets.Field)]
	public class Manual : Attribute
	{
	}

	/// <summary>
	/// Use to set a field only ONCE to a shader. If you need dynamic control implement IFieldsWithDirtyState and set fields dirty individually
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class Once : Attribute
	{
		
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class TransformInfo : Attribute
	{
		// TODO: would be nice to be able to set position+scale in one structure

		public enum DataType
		{
			Undefined = 0,
			Position = 1,
			Rotation = 2,
			Scale = 3,
			LocalPosition = 4,
			LocalRotation = 5,
			LocalScale = 6,
			RotationQuaternion = 7,
			LocalRotationQuaternion = 8,
			WorldMatrix = 10,
			LocalMatrix = 11
		}

		public DataType Type;

		public Vector4 GetVector4(Transform t)
		{
			switch (Type)
			{
				default:
				case DataType.Position:
					var pos = t.position;
					return new Vector4(pos.x, pos.y, pos.z, 1);
				case DataType.Rotation:
					return t.rotation.eulerAngles;
				case DataType.Scale:
					return t.lossyScale;
				case DataType.LocalPosition:
					return t.localPosition;
				case DataType.LocalRotation:
					return t.localRotation.eulerAngles;
				case DataType.LocalScale:
					return t.localScale;
				case DataType.RotationQuaternion:
					var rot = t.rotation;
					return new Vector4(rot.x, rot.y, rot.z, rot.w);
				case DataType.LocalRotationQuaternion:
					var localRot = t.localRotation;
					return new Vector4(localRot.x, localRot.y, localRot.z, localRot.w);
			}
		}

		public Matrix4x4 GetMatrix(Transform t)
		{
			switch (Type)
			{
				default:
				case DataType.WorldMatrix:
					return t.localToWorldMatrix;
				case DataType.LocalMatrix:
					return t.worldToLocalMatrix;
			}
		}

		public void SetDefault(string shaderType)
		{
			if (Type != DataType.Undefined) return;
			switch (shaderType)
			{
				case "float3":
				case "float4":
					Type = DataType.Position;
					break;
				case "float4x4":
					Type = DataType.WorldMatrix;
					break;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class TextureInfo : Attribute
	{
		public readonly int Width;
		public readonly int Height;
		public GraphicsFormat GraphicsFormat;
		public TextureFormat TextureFormat;
		public int Depth = 0;
		public FilterMode FilterMode = FilterMode.Bilinear;

		public bool HasValidSize => Width > 0 && Height > 0;
		
		public TextureInfo(){}

		public TextureInfo(int width, int height)
		{
			Width = width;
			Height = height;
		}

		public RenderTextureDescription ToRenderTextureDescription()
		{
			var desc = new RenderTextureDescription
			{
				Width = Width,
				Height = Height,
				Depth = Depth,
				GraphicsFormat = GraphicsFormat,
				FilterMode = FilterMode
			};
			return desc;
		}
	}


	[AttributeUsage(AttributeTargets.Field)]
	public abstract class BufferInfo : Attribute
	{
		public int Size;
		public int Stride;
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ComputeBufferInfo : BufferInfo
	{
		public ComputeBufferType Type;
		public ComputeBufferMode Mode;

		public ComputeBufferInfo(){}

		public ComputeBufferInfo(int size, int stride)
		{
			this.Size = size;
			this.Stride = stride;
		}
	}
}