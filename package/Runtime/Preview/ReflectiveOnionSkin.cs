// namespace Needle.Timeline
// {
// 	internal class ReflectiveOnionSkin : IOnionSkin
// 	{
// 		private ClipInfoViewModel viewModel;
// 		private bool canRenderOnionSkin, fieldsParsed;
// 		private object instance;
// 		
// 		internal ReflectiveOnionSkin(ClipInfoViewModel viewModel)
// 		{
// 			this.viewModel = viewModel;
// 			this.instance = viewModel.Script;
// 		}
// 		
// 		public void RenderOnionSkin(IOnionData data)
// 		{
// 			if (!fieldsParsed)
// 			{
// 				fieldsParsed = true;
// 				ParseFields();
// 			}
// 			if (!canRenderOnionSkin) return;
// 			
// 		}
//
// 		private void ParseFields()
// 		{
// 		}
// 	}
//
// 	internal interface IOnionRenderer
// 	{
// 		void Render(IOnionData data);
// 	}
// }