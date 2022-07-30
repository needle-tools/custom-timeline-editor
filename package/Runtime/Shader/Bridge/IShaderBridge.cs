
namespace Needle.Timeline
{
	/// <summary>
	/// Is responsible for actually passing a value to the shader
	/// </summary>
	public interface IShaderBridge
	{
		bool SetValue(IBindingContext context);
		// bool TryGetValue(object instance, out object value);
	}
}