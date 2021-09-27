namespace Needle.Timeline
{
	public interface ICommand
	{
		void Execute();
		void Undo();
	}
}