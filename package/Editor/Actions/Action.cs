namespace Needle.Timeline
{
	public abstract class Command : ICommand
	{
		private bool isDone;
		
		public void Execute()
		{
			if (isDone) return;
			isDone = true;
			OnExecute();
		}

		public void Undo()
		{
			if (!isDone) return;
			isDone = false;
			OnUndo();
		}

		protected abstract void OnExecute();
		protected abstract void OnUndo();
	}
}