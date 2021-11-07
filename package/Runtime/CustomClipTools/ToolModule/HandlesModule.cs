using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline
{
	public class HandlesModule : CoreToolModule
	{
		// TODO: how can we delete elements

		public override bool CanModify(Type type)
		{
			return typeof(ICustomControls).IsAssignableFrom(type);
		}

		protected override IList<Type> SupportedTypes => Type.EmptyTypes;

		public override bool WantsInput(InputData input)
		{
			return true;
		}

		protected override ToolInputResult OnModifyValue(InputData input, ref ModifyContext context, ref object value)
		{
			if (value is ICustomControls cc)
			{
				var res = cc.OnCustomControls(input, this);
				if (res) return ToolInputResult.Success;
			}
			return ToolInputResult.Failed;
		}
	}
}