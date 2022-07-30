using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Needle.Timeline.ResourceProviders;
using UnityEngine;

namespace Needle.Timeline
{
	[ShaderBridge(typeof(IList))]
	public class CollectionBridge : IShaderBridge
	{
		private FieldInfo list_backingArray;
		private Array own_backingArrayArray;

		private bool didSearchOnceAttribute = false;
		private bool hasOnceAttribute;
		private bool didSet = false;

		private bool didCheckType;
		private bool typeIsBlittable;
		private Type contentType;

		public bool SetValue(IBindingContext context)
		{
			if (didSet && hasOnceAttribute) return true;

			var field = context.Field;
			if (!didSearchOnceAttribute)
			{
				didSearchOnceAttribute = true;
				hasOnceAttribute = field.GetCustomAttribute<Once>() != null;
			}

			var instance = context.Instance;
			if (didSet && instance is IFieldsWithDirtyState state && !state.IsDirty(field.Name))
			{
				return true;
			}
			didSet = true;

			var shaderField = context.ShaderField;
			var resources = context.Resources;
			var shaderInfo = context.ShaderInfo;
			var value = field.GetValue(instance);
			var desc = new ComputeBufferDescription();
			desc.Name = shaderField.FieldName;
			desc.Stride = shaderField.Stride;
			desc.Type = shaderField.RandomWrite.GetValueOrDefault() ? ComputeBufferType.Structured : ComputeBufferType.Default;

			var list = value as IList;
			ComputeBuffer buffer;
			if (list == null || list.Count <= 0)
			{
				desc.Size = 1;
				buffer = resources.ComputeBufferProvider.GetBuffer(shaderField.FieldName, desc);
				shaderInfo.Shader.SetBuffer(context.KernelIndex, shaderField.FieldName, buffer);
				shaderInfo.Shader.SetInt(shaderField.FieldName + "Count", 0);
				return true;
			}

			desc.Size = list.Count;
			buffer = resources.ComputeBufferProvider.GetBuffer(shaderField.FieldName, desc);
			if (list is Array arr)
			{
				if (!didCheckType)
				{
					didCheckType = true;
					contentType = arr.GetType().GetElementType();
					if (contentType == null) throw new Exception("Unknown content type: " + arr);
					if (contentType.IsValueType)
						typeIsBlittable = true;
				}

				if (typeIsBlittable)
					buffer.SetData(arr);
				else
				{
					UpdateOwnBackingArray(field, shaderField, arr, contentType);
					buffer.SetData(own_backingArrayArray, 0, 0, arr.Length);
				}
			}
			else
			{
				if (!didCheckType)
				{
					didCheckType = true;
					var type = list.GetType();
					if (type.IsGenericType)
					{
						contentType = type.GetGenericArguments().First();
					}
					else throw new Exception("Unknown content type: " + list);
					if (contentType == null) throw new Exception("Unknown content type: " + list);
					if (contentType.IsValueType)
						typeIsBlittable = true;
				}

				// TODO: find better way of setting content to buffer
				// TODO: this works only for value types, and e.g. a list of transforms would fail
				list_backingArray ??= list.GetType().GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
				var backingArray = list_backingArray.GetValue(list) as Array;

				if (typeIsBlittable)
					buffer.SetData(backingArray, 0, 0, list.Count);
				else
				{
					UpdateOwnBackingArray(field, shaderField, backingArray, contentType);
					buffer.SetData(own_backingArrayArray, 0, 0, backingArray?.Length ?? 0);
				}
			}
			shaderInfo.Shader.SetBuffer(context.KernelIndex, shaderField.FieldName, buffer);
			shaderInfo.Shader.SetInt(shaderField.FieldName + "Count", list.Count);
			return true;
		}


		private bool triedGettingTransformInfo;
		private TransformInfo transformInfo;

		private void UpdateOwnBackingArray(FieldInfo fi, ComputeShaderFieldInfo field, IList list, Type contentType)
		{
			if (typeof(Transform).IsAssignableFrom(contentType))
			{
				if (!triedGettingTransformInfo)
				{
					triedGettingTransformInfo = true;
					transformInfo = fi.GetCustomAttribute<TransformInfo>();
				}

				switch (field.GenericTypeName)
				{
					case "float3":
						if (transformInfo != null)
							InternalUpdate<Transform, Vector3>(e => transformInfo.GetVector4(e));
						else
							InternalUpdate<Transform, Vector3>(e => e.position);
						break;
					case "float4":
						if (transformInfo != null)
							InternalUpdate<Transform, Vector4>(e => transformInfo.GetVector4(e));
						else
							InternalUpdate<Transform, Vector4>(e => e.position);
						break;
					case "float4x4":
						if (transformInfo != null)
							InternalUpdate<Transform, Matrix4x4>(e => transformInfo.GetMatrix(e));
						else
							InternalUpdate<Transform, Matrix4x4>(e => e.localToWorldMatrix);
						break;
				}
			}

			void InternalUpdate<T, TEntryType>(Func<T, TEntryType> getValue)
			{
				if (own_backingArrayArray == null || own_backingArrayArray.Length <= list.Count)
				{
					own_backingArrayArray = new TEntryType[list.Count * 2];
				}
				var arr = own_backingArrayArray as TEntryType[];
				for (var index = 0; index < list.Count; index++)
				{
					var entry = list[index];
					arr[index] = getValue((T)entry);
				}
			}
		}
	}
}