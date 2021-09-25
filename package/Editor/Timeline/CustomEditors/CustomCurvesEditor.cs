using UnityEditor.Experimental.TerrainAPI;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline
{
	[CustomTimelineEditor(typeof(CodeControlTrack))]
	public class CustomCurvesEditor : UnityEditor.Timeline.CustomCurvesEditor
	{
		protected override void OnDrawHeader(Rect rect)
		{
			GUI.Label(rect, "Custom header");
		}

		private ICustomKeyframe _dragging;

		protected override void OnDrawTrack(Rect rect)
		{
			var useEvent = false;
			foreach (var clip in EnumerateClips())
			{
				if (clip.asset is CodeControlAsset code)
				{
					var viewModel = code.viewModel;
					if (viewModel?.clips != null)
					{
						var row = 0;
						foreach (var curves in viewModel.clips)
						{
							if (curves is IKeyframesProvider prov)
							{
								foreach (var kf in prov.Keyframes)
								{
									#region Draw
									var r = new Rect();
									r.x = TimeToPixel(clip.start + kf.time / clip.timeScale);
									r.width = 8;
									r.x -= r.width * .5f;
									r.height = r.width;
									r.y = rect.y + r.height;
									r.y += row * r.height * 1.2f;
									var highlight = kf.IsSelected();
									var col = highlight ? Color.yellow : Color.gray;
									col.a = .7f;
									GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true,
										1, col, 0, 4);
									#endregion

									if (Event.current.button == 0)
									{
										switch (Event.current.type)
										{
											case EventType.MouseDown:
												if (r.Contains(Event.current.mousePosition))
												{
													useEvent = true;
													_dragging = kf;
												}

												break;
											case EventType.MouseDrag:
												if (_dragging == kf)
												{
													kf.Select(curves);
													var timeDelta = PixelDeltaToDeltaTime(Event.current.delta.x * (float)clip.timeScale);
													kf.time += timeDelta;
													Repaint();
													UpdatePreview();
													useEvent = true;
												}

												break;
											case EventType.MouseUp:
												_dragging = null;
												if (r.Contains(Event.current.mousePosition))
												{
													useEvent = true;
													kf.Select(curves);
												}

												break;
										}
									}
								}

								++row;
							}
						}
					}
				}
			}

			if (useEvent)
				Event.current.Use();
		}
	}
}