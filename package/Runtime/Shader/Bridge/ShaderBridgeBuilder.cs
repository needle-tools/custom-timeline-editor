using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable enable

namespace Needle.Timeline
{
	public static class ShaderBridgeBuilder
	{
		public static IShaderBridge? BuildMapping(FieldInfo field)
		{
			if (typeof(ComputeBuffer).IsAssignableFrom(field.FieldType))
				return new ComputeBufferBridge();

			if (typeof(IList).IsAssignableFrom(field.FieldType))
				return new CollectionBridge();

			if (typeof(Transform).IsAssignableFrom(field.FieldType))
				return new TransformBridge();

			if (typeof(Texture).IsAssignableFrom(field.FieldType))
				return new TextureBridge();

			if (PrimitiveBridge.Types.Any(t => t.IsAssignableFrom(field.FieldType)))
				return new PrimitiveBridge();
			
			return null;
		}
	}
}