#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline
{
	public class ToolInputInfo
	{
		public Vector3? WorldPosition;
		public Vector3? LastWorldPosition;

		private Vector3? deltaWorld;
		public Vector3? DeltaWorld
		{
			get
			{
				if (deltaWorld == null && WorldPosition.HasValue && LastWorldPosition.HasValue) 
					return deltaWorld = WorldPosition.Value - LastWorldPosition.Value;
				return deltaWorld.GetValueOrDefault();
			}
		}

		public Vector2 ScreenPosition;
		public Vector2 ScreenDelta;
	}
	
	public interface IToolModule
	{
		bool CanModify(Type type);
	}

	public abstract class ToolModule : IToolModule
	{
		private static bool modulesInit;
		private static readonly List<ToolModule> modules = new List<ToolModule>();

		public static IReadOnlyList<ToolModule> Modules
		{
			get
			{
				if (!modulesInit)
				{
					modulesInit = true;
					foreach (var mod in RuntimeTypeCache.TypesDerivingFrom<ToolModule>())
					{
						if (mod.IsAbstract || mod.IsInterface) continue;
						modules.Add(Activator.CreateInstance(mod) as ToolModule);
					}
				}
				return modules;
			}
		}

		public static void GetModulesSupportingType(Type type, IList<IToolModule> list)
		{
			
		}

		public abstract bool CanModify(Type type);
	}

	public abstract class ToolModule<T> : ToolModule
	{
		public sealed override bool CanModify(Type type)
		{
			return OnCanModify(type);
		}

		public void BeginInput(T target)
		{
		}

		public void UpdateInput(T target)
		{
		}

		public void EndInput(T target)
		{
		}

		protected abstract bool OnCanModify(Type type);
	}

	public class GenerateVector3Module : ToolModule<Vector3>
	{
		protected override bool OnCanModify(Type type)
		{
			return typeof(Vector3).IsAssignableFrom(type);
		}
	}
}