namespace Editor.Interfaces
{
	public interface ICanInterpolate<T>
	{
		T Interpolate(T v0, T v1, float t);
	}
}