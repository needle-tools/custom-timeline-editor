using System.Linq;
using System.Net.Configuration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Needle.Timeline
{
	[CustomTimelineEditor(typeof(CodeControlTrack))]
	public class CodeControlCustomCurvesEditor : CustomCurvesEditor
	{
		protected override void OnDrawHeader(Rect rect)
		{
			GUI.Label(rect, "Custom header");
		}

		private UnityEditor.Editor ed;
		protected override void OnDrawTrack(Rect rect)
		{
			// GUI.Label(rect, "Custom track");
			// GUI.DrawTexture(GetRangeRect(rect.y + rect.height - 2, 2), Texture2D.redTexture, ScaleMode.StretchToFill, false);

			foreach (var clip in EnumerateClips())
			{
				if (clip.asset is CodeControlAsset code)
				{
					var viewModel = code.viewModel;
					if (viewModel?.clips != null)
					{
						int row = 0;
						foreach (var curves in viewModel.clips)
						{
							if (curves is IKeyframesProvider prov)
							{
								foreach (var kf in prov.Keyframes)
								{
									Debug.Log(clip.start.ToString("0.0") + ": " + kf.time.ToString("0.0"));
									var r = new Rect();
									r.x = TimeToPixel(clip.start + kf.time / clip.timeScale);
									r.width = 8;
									r.x -= r.width * .5f;
									r.height = r.width;
									r.y = rect.y + r.height;
									r.y += row * r.height * 1.2f;
									var highlight = SelectedClip == null || clip == SelectedClip;
									highlight &= !kf.AnySelected();
									highlight |= kf.IsSelected();
									var col = highlight ? Color.yellow : Color.gray;
									GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true,
										1, col, 0, 4);
									if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
									{
										if (r.Contains(Event.current.mousePosition))
										{
											Debug.Log("Select " + kf.time);
											Event.current.Use();
											KeyframeInspectorHelper.Select(kf);
										}
									}
								}

								++row;
							}
						}
					}
				}
			}
		}
	}
}