#nullable enable
using System;
using System.Collections.Generic;

namespace Needle.Timeline
{
	public interface IRegistry
	{
		public bool TryFind(Predicate<Type> test, out Type match);
		public IList<Type> GetAll();
	}
}