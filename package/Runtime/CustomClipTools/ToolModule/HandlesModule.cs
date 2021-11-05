using System;

namespace Needle.Timeline
{
	public class HandlesModule : ToolModule
	{
		// TODO: how can we delete elements

		public override bool CanModify(Type type)
		{
			return typeof(ICustomControls).IsAssignableFrom(type);
		}

		public override bool WantsInput(InputData input)
		{
			return true;
		}

		public override bool OnModify(InputData input, ref ToolData toolData)
		{
			if (toolData.Value == null) return false;
			if (toolData.Value is ICustomControls mod)
			{
				return mod.OnCustomControls(input, this);
			}
			return false;
		}
	}
}