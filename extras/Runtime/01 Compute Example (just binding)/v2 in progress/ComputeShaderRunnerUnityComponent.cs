using UnityEngine;

namespace Needle.Timeline.Samples
{
	[ExecuteAlways]
	public class ComputeShaderRunnerUnityComponent : MonoBehaviour
	{
		public ComputeShader Shader;

		protected ComputeShaderRunner Runner { get; private set; }

		protected virtual void OnEnable() => CreateRunner();
		protected virtual void OnValidate() => CreateRunner();

		private void CreateRunner()
		{
			if (Runner?.Shader == Shader) return;
			Runner?.Dispose();
			if (Shader) Runner = new ComputeShaderRunner(this, Shader);
		}

		protected virtual void Update()
		{
			// by default just run with default kernel size
			// which is determined by the usage in your shader e.g. if you write to textures
			Runner?.RunAll();
		}
	}
}

