using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.U2D;

namespace GameMain.Systems
{
    public interface IResLoadSystem:ISystem
    {
        void LoadSprite(string spriteName,Action<Sprite>  onLoadCallback);
        void ReleaseSprite(string spriteName);
        void ReleaseAllSprites();
        void LoadAtlas(string atlasName,Action<SpriteAtlas>  onLoadCallback);
        void ReleaseAtlas(string atlasName);
        void ReleaseAllAtlas();
        void LoadPrefab(string prefabName,Action<GameObject>  onLoadCallback);
        void ReleasePrefabs(string prefabName);
        void ReleaseAllPrefabs();
    }
    
    
    public class ResLoadSystem : AbstractSystem,IResLoadSystem
    {

        private ResLoader _resLoader;
        private readonly Dictionary<string, Sprite> _spriteCache = new();
        private readonly Dictionary<string, SpriteAtlas> _atlasCache = new();
        private readonly Dictionary<string,GameObject> _prefabCache = new();
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

        public void LoadSprite(string spriteName,Action<Sprite>  onLoadCallback)
        {
            if(!EnsureReady())
                return;
            _resLoader.Add2Load(spriteName, (success, res) =>
            {
                if (!success)
                {
                    Debug.LogWarning("图片加载失败");
                    return;
                }

                if (_spriteCache.TryGetValue(spriteName, out Sprite sprite))
                {
                    onLoadCallback?.Invoke(sprite);
                    return;
                }
                var texture2D=res.Asset as Texture2D;
                if (texture2D == null)
                {
                    Debug.Log("加载图片失败");
                    return;
                }
                Sprite spr=Sprite.Create(texture2D,new Rect(0,0,texture2D.width,texture2D.height),new Vector2(0.5f,0.5f));
                onLoadCallback?.Invoke(spr);
                _spriteCache[spriteName] = spr;
            });
            _resLoader.LoadAsync();
        }

        public void ReleaseSprite(string spriteName)
        {
            
        }

        public void ReleaseAllSprites()
        {
            
        }

        public void LoadAtlas(string atlasName,Action<SpriteAtlas>  onLoadCallback)
        {
            if(!EnsureReady())
                return;
            _resLoader.Add2Load(atlasName, (success, res) =>
            {
                if (!success)
                {
                    Debug.LogWarning("图集加载失败");
                    return;
                }

                if (_atlasCache.TryGetValue(atlasName, out SpriteAtlas atlas))
                {
                    onLoadCallback?.Invoke(atlas);
                    return;
                }
                var spriteAtlas=res.Asset as SpriteAtlas;
                if (spriteAtlas == null)
                {
                    Debug.Log("加载图集失败");
                    return;
                }
                onLoadCallback?.Invoke(spriteAtlas);
                _atlasCache[atlasName] = spriteAtlas;
            });
            _resLoader.LoadAsync();
        }

        public void ReleaseAtlas(string atlasName)
        {
            
        }

        public void ReleaseAllAtlas()
        {
            
        }

        public void LoadPrefab(string prefabName,Action<GameObject>  onLoadCallback)
        {
            if(!EnsureReady())
                return;
        }

        public void ReleasePrefabs(string prefabName)
        {
            
        }

        public void ReleaseAllPrefabs()
        {
            
        }
    }
}

