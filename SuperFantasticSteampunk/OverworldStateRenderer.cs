﻿using System;
using Microsoft.Xna.Framework;

namespace SuperFantasticSteampunk
{
    abstract class OverworldStateRenderer
    {
        #region Instance Properties
        protected OverworldState overworldState { get; private set; }
        #endregion

        #region Constructors
        public OverworldStateRenderer(OverworldState overworldState)
        {
            if (overworldState == null)
                throw new Exception("OverworldState cannot be null");
            this.overworldState = overworldState;
        }
        #endregion

        #region Instance Methods
        public virtual void Finish()
        {
        }

        public virtual void Pause()
        {
        }

        public virtual void Resume()
        {
        }

        public abstract void Update(Delta delta);

        public abstract void BeforeDraw(Renderer renderer);

        public abstract void AfterDraw(Renderer renderer);
        #endregion
    }
}
