using UnityEngine;

namespace Needle.Timeline
{
	internal class OnionSkinRenderer
	{
		private readonly ClipInfoViewModel viewModel;
		private IOnionSkin onion;

		internal OnionSkinRenderer(ClipInfoViewModel viewModel)
		{
			this.viewModel = viewModel;
		}
		
		public void Render()
		{
			if (onion == null)
			{
				if (viewModel.Script is IOnionSkin o)
				{
					onion = o;
				}
				else
				{
					// TODO: create automatic onion skin renderer
					return;
				}
			}
			
			var time = (float)viewModel.ClipTime;
			var clips = viewModel.clips;
			var values = viewModel.values;
			
			var renderPrev = false;
			for (var index = 0; index < clips.Count; index++)
			{
				var clip = clips[index];
				var prev = default(IReadonlyCustomKeyframe);
				foreach (var kf in clip.Keyframes)
				{
					var diff = kf.time - time;
					if (diff < 0 && diff < -Mathf.Epsilon)
					{
						prev = kf;
					}
					else if (kf.time >= time)
					{
						if (prev != null)
						{
							renderPrev = true;
							values[index].SetValue(prev.value);
							break;
						}
					}
				}
			}

			if (renderPrev)
				onion.RenderOnionSkin(new OnionData(-1));

			var renderNext = false;
			for (var index = 0; index < clips.Count; index++)
			{
				var clip = clips[index];
				foreach (var kf in clip.Keyframes)
				{
					if (kf.time > time)
					{
						renderNext = true;
						values[index].SetValue(kf.value);
						break;
					}
				}
			}
			if (renderNext)
				onion.RenderOnionSkin(new OnionData(1));
		}
	}
}