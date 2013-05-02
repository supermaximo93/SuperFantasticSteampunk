﻿using System;
using Microsoft.Xna.Framework;
using Spine;

namespace SuperFantasticSteampunk
{
    abstract class BattleState
    {
        #region Instance Properties
        public Battle Battle { get; private set; }
        public BattleStateRenderer BattleStateRenderer { get; protected set; }
        #endregion

        #region Constructors
        protected BattleState(Battle battle)
        {
            if (battle == null)
                throw new Exception("Battle cannot be null");
            Battle = battle;
        }
        #endregion

        #region Instance Methods
        public abstract void Start();
        public abstract void Finish();

        public virtual void Pause()
        {
        }

        public virtual void Resume(BattleState previousBattleState)
        {
        }

        public abstract void Update(GameTime gameTime);

        public void ChangeState(BattleState state)
        {
            if (Battle.CurrentBattleState == this)
                Battle.ChangeState(state);
        }

        public void PushState(BattleState state)
        {
            if (Battle.CurrentBattleState == this)
                Battle.PushState(state);
        }

        public void PopState()
        {
            if (Battle.CurrentBattleState == this)
                Battle.PopState();
        }
        #endregion
    }
}
