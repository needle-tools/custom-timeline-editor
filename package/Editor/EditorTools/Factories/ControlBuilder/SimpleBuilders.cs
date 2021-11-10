using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	public class TextBuilder : SimpleGenericBuilder<string>
	{
		protected override BaseField<string> GetField()
		{
			return new TextField();
		}
	}

	public class DoubleBuilder : SimpleGenericBuilder<double>
	{
		protected override BaseField<double> GetField()
		{
			return new DoubleField();
		}
	}
	public class ToggleBuilder : SimpleGenericBuilder<bool>
	{
		protected override BaseField<bool> GetField()
		{
			return new Toggle();
		}
	}

	public class Vector2IntBuilder : SimpleGenericBuilder<Vector2Int>
	{
		protected override BaseField<Vector2Int> GetField()
		{
			return new Vector2IntField();
		}
	}

	public class Vector2Builder : SimpleGenericBuilder<Vector2>
	{
		protected override BaseField<Vector2> GetField()
		{
			return new Vector2Field();
		}
	}

	public class Vector3IntBuilder : SimpleGenericBuilder<Vector3Int>
	{
		protected override BaseField<Vector3Int> GetField()
		{
			return new Vector3IntField();
		}
	}

	public class Vector3Builder : SimpleGenericBuilder<Vector3>
	{
		protected override BaseField<Vector3> GetField()
		{
			return new Vector3Field();
		}
	}

	public class Vector4Builder : SimpleGenericBuilder<Vector4>
	{
		protected override BaseField<Vector4> GetField()
		{
			return new Vector4Field();
		}
	}
}