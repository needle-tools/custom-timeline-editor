using System;

namespace Needle.Timeline
{
	public interface IToolModule
	{
		bool WantsInput(InputData input);
		bool CanModify(Type type);
		bool OnModify(InputData input, ref ToolData toolData);
		void Reset();
		void OnDrawGizmos(InputData modInput);
	}
}