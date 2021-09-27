using System.Threading.Tasks;
using UnityEditor;

namespace Needle.Timeline
{
	[CustomEditor(typeof(CodeControlAsset))]
	public class CodeControlAssetEditor : UnityEditor.Editor
	{
		private async void OnEnable()
		{
		}
		
		private void OnDisable()
		{
		}
	}
}