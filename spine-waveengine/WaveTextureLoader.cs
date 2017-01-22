#region File Description
//-----------------------------------------------------------------------------
// WaveTextureLoader
//
// Copyright © 2014 Wave Corporation
// Use is subject to license terms.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Spine;
using System.IO;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
#endregion

namespace WaveEngine.Spine
{
    /// <summary>
    /// Loader for wpk textures
    /// </summary>
    internal class WaveTextureLoader : TextureLoader
    {
        /// <summary>
        /// The assets
        /// </summary>
        private AssetsContainer assets;

        #region Initialize
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveTextureLoader" /> class.
        /// </summary>
        /// <param name="assets">The assets.</param>
        public WaveTextureLoader(AssetsContainer assets)
        {
            this.assets = assets;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads the specified page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="path">The path.</param>
        public void Load(AtlasPage page, string path)
        {
            path = Path.ChangeExtension(path, ".wpk");
            Texture2D texture = this.assets.LoadAsset<Texture2D>(path);
            page.rendererObject = texture;
            page.width = texture.Width;
            page.height = texture.Height;
        }

        /// <summary>
        /// Unloads the specified texture.
        /// </summary>
        /// <param name="texture">The texture.</param>
        public void Unload(object texture)
        {
            this.assets.UnloadAsset(((Texture2D)texture).AssetPath);
        }
        #endregion
    }
}
