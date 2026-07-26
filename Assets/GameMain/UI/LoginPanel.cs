using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace GameMain.UI
{
	public class LoginPanelData : UIPanelData
	{
	}
	public partial class LoginPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as LoginPanelData ?? new LoginPanelData();
			// please add init code here
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}
	}
}
