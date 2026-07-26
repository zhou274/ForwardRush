using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace GameMain.UI
{
	// Generate Id:ae27f56b-95d9-4081-b2f0-76d09c9fcc8c
	public partial class LoginPanel
	{
		public const string Name = "LoginPanel";
		
		
		private LoginPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			
			mData = null;
		}
		
		public LoginPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		LoginPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new LoginPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
