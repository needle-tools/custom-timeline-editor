using System.Threading.Tasks;
using UnityEditor;

namespace Needle.Timeline
{
	[CustomEditor(typeof(CodeControlAsset))]
	public class CodeControlAssetEditor : UnityEditor.Editor
	{
		private async void OnEnable()
		{
			var asset = target as CodeControlAsset;
			// workaround until asset not being initialized automatically when reload happens
			if (asset?.viewModel == null) await Task.Delay(1);
			ToolsHandler.OnEnable(asset);
		}
		
		private void OnDisable()
		{
			ToolsHandler.OnDisable();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			ToolsHandler.OnInspectorGUI();
		}

		private void OnSceneGUI()
		{
			ToolsHandler.OnSceneGUI();
		}
	}
}