#nullable enable
using System;
using System.Collections;

namespace Needle.Timeline
{
	public interface IArgument
	{
		string? Name { get; }
		Type Type { get; }
		object Value { get; }
		bool Match(string name, Type type);
	}

	public readonly struct Argument : IArgument
	{
		public string? Name { get; }
		public Type Type { get; }
		public object Value { get; }

		public bool Match(string name, Type type)
		{ 
			if (Name != null && name != Name) return false;
			if (!strictType && type.IsAssignableFrom(Type)) return true;
			return type == Type; 
		}

		private bool strictType { get; }

		public Argument(string? name, object value, Type? type = null, bool strictType = false)
		{
			Name = name;
			Type = type ?? value.GetType();
			Value = value;
			this.strictType = strictType;
			if (!Type.IsInstanceOfType(value))
				throw new Exception("Type value mismatch");
		}
	}
}