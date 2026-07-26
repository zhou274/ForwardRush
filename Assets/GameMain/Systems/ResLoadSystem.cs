using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace GameMain.Systems
{
    public interface IResLoadSystem:ISystem
    {
        void LoadSprite();
        void LoadAtlas();
        void LoadPrefab();
    }
    
    
    public class ResLoadSystem : AbstractSystem,IResLoadSystem
    {

        private ResLoader _resLoader;
        private bool _isInit = false;
        protected override void OnInit()
        {
            if (PlatformCheck.IsWeixinMiniGame || PlatformCheck.IsWebGL)
            {
                ResKit.InitAsync().ToAction().StartGlobal(() =>
                {
                    _resLoader = ResLoader.Allocate();
                    _isInit = true;
                    Debug.Log("ResLoadSystem Init");
                });
            }
        }
        
        private bool EnsureReady()
        {
            if (_resLoader == null || !_isInit)
            {
                Debug.LogWarning("ResLoader尚未初始化完成，请等待ResKit异步初始化");
                return false;
            }
            return true;
        }

        public void LoadSprite()
        {
            
        }

        public void LoadAtlas()
        {
            
        }

        public void LoadPrefab()
        {
            
        }
        
        

        
    }
}

