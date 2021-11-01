using System;

namespace Needle.Timeline.ResourceProviders
{
	public interface IResourceProvider : IDisposable
	{
		void ClearCaches();
		IComputeBufferProvider ComputeBufferProvider { get; }
		IRenderTextureProvider RenderTextureProvider { get; }
	}

	public class ResourceProvider : IResourceProvider
	{
		public static IResourceProvider CreateDefault() => new ResourceProvider(
			new DefaultComputeBufferProvider(), 
			new DefaultRenderTextureProvider()
		);
		
		public ResourceProvider(IComputeBufferProvider computeBufferProvider, IRenderTextureProvider renderTextureProvider)
		{
			ComputeBufferProvider = computeBufferProvider;
			RenderTextureProvider = renderTextureProvider;
		}

		public void ClearCaches()
		{
			RenderTextureProvider.ClearCaches();
			ComputeBufferProvider.ClearCaches();
		}

		public IComputeBufferProvider ComputeBufferProvider { get; }
		public IRenderTextureProvider RenderTextureProvider { get; }

		public void Dispose()
		{
			ComputeBufferProvider?.Dispose();
			RenderTextureProvider?.Dispose();
		}
	}
}