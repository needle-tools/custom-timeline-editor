using UnityEngine;

namespace Needle.Timeline.Commands
{
	public class DeleteTrackCommand : Command
	{
		private Object asset;
		private JsonContainer jsonContainer; 

		public DeleteTrackCommand(Object asset, JsonContainer container)
		{
			this.asset = asset;
			this.jsonContainer = container; 
		}
		
		protected override void OnRedo()
		{
			Debug.Log("REDO DELETION");
		}

		protected override void OnUndo()
		{
			Debug.Log("UNDO DELETION");
		}
	}
}