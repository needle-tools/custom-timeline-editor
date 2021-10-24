using Needle.Timeline.Shader;

namespace Needle.Timeline
{
	public interface IShaderBridge
	{
		bool SetValue(IBindingContext context);
		// bool TryGetValue(object instance, out object value);
	}
}