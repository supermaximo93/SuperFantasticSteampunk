﻿using Microsoft.Xna.Framework;

namespace SuperFantasticSteampunk.BattleStates
{
    class Intro : BattleState
    {
        #region Constructors
        public Intro(Battle battle)
            : base(battle)
        {
        }
        #endregion

        #region Instance Methods
        public override void Start()
        {
            throw new System.NotImplementedException();
        }

        public override void Finish()
        {
            ChangeState(new Think(battle));
        }

        public override void Update(GameTime gameTime)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}