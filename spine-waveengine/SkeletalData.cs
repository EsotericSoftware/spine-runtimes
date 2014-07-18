#region File Description
//-----------------------------------------------------------------------------
// SkeletalData
//
// Copyright © 2014 Wave Corporation
// Use is subject to license terms.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Spine;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
#endregion

namespace WaveEngine.Spine
{
    /// <summary>
    /// Hold all skeletal 2D info
    /// </summary>
    public class SkeletalData : Component
    {
        /// <summary>
        ///     Number of instances of this component created.
        /// </summary>
        private static int instances;

        /// <summary>
        /// The atlas path
        /// </summary>
        private string atlasPath;

        /// <summary>
        /// The atlas
        /// </summary>
        private Atlas atlas;

        /// <summary>
        /// The transform2 D
        /// </summary>
        [RequiredComponent]
        public Transform2D Transform2D;

        #region Properties

        /// <summary>
        /// Gets the atlas.
        /// </summary>
        /// <value>
        /// The atlas.
        /// </value>
        public Atlas Atlas
        {
            get { return this.atlas; }
        }

        #endregion

        #region Initialize
        /// <summary>
        /// Initializes a new instance of the <see cref="SkeletalData" /> class.
        /// </summary>
        /// <param name="atlasPath">The atlas path.</param>
        public SkeletalData(string atlasPath)
            : base("skeletalData" + instances++)
        {
            this.atlasPath = atlasPath;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Performs further custom initialization for this instance.
        /// </summary>
        /// <remarks>
        /// By default this method does nothing.
        /// </remarks>
        protected override void Initialize()
        {
            base.Initialize();

            this.atlas = new Atlas(this.atlasPath, new WaveTextureLoader(this.Assets));
        }
        #endregion
    }
}
