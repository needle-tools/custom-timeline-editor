using UnityEngine;

namespace Needle.Timeline.Samples
{
	[ExecuteAlways]
	public class BindingBaseClass : MonoBehaviour
	{
		private ComputeShaderRunner _runner;
		
		// The shader to use
		public ComputeShader Shader;
		
		// the output
		[TextureInfo(512, 512)] 
		public RenderTexture Result;

		private void OnEnable() => CreateRunner();
		private void OnValidate() => CreateRunner();

		private void CreateRunner()
		{
			_runner?.Dispose();
			if (Shader) _runner = new ComputeShaderRunner(this, Shader);
		}

		private void Update()
		{
			if (_runner == null) return;
			OnDispatch(_runner);
			ShowTexture();
		}

		protected virtual void OnDispatch(ComputeShaderRunner runner)
		{
			_runner.RunAll();
		}

		[Header("Visualization")] public Renderer Renderer;

		private void ShowTexture()
		{
			if (Renderer?.sharedMaterial)
				Renderer.sharedMaterial.mainTexture = Result;
		}
	}
}

