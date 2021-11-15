using System;
using System.Collections.Generic;
using System.Linq;

namespace Needle.Timeline
{
	public interface IInputCommandHandler
	{
		void RegisterCommand(ICommand command);
		bool HasCommand(ICommand command);
		bool HasCommand(Predicate<ICommand> pred);
		int Count { get; }
		internal void FlushCommands(string name);
		internal void ClearCommands();
	}

	public class TimelineInputCommandHandler : IInputCommandHandler
	{
		private readonly List<ICommand> commands = new List<ICommand>();
		
		public void RegisterCommand(ICommand command)
		{
			commands.Add(command);
		}

		public bool HasCommand(ICommand command)
		{
			return commands.Any(c => c.Equals(command));
		}

		public bool HasCommand(Predicate<ICommand> pred)
		{
			return commands.Any(c => pred(c));
		}

		public int Count => commands.Count;

		void IInputCommandHandler.FlushCommands(string name)
		{
			var cmp = commands.ToCompound(name ?? (commands.Count + " edits"));
			CustomUndo.Register(cmp);
			commands.Clear();
		}

		void IInputCommandHandler.ClearCommands()
		{
			commands.Clear();
		}
	}
}