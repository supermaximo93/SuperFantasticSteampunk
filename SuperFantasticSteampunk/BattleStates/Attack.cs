﻿using System;
using Microsoft.Xna.Framework;
using Spine;

namespace SuperFantasticSteampunk.BattleStates
{
    class Attack : BattleState
    {
        #region Instance Fields
        private ThinkAction thinkAction;
        private ScriptRunner scriptRunner;
        private float scriptStartTime;
        private float scriptStartTimer;
        #endregion

        #region Constructors
        public Attack(Battle battle, ThinkAction thinkAction)
            : base(battle)
        {
            if (thinkAction == null)
                throw new Exception("ThinkAction cannot be null");
            this.thinkAction = thinkAction;
            scriptStartTime = 0.0f;
            scriptStartTimer = 0.0f;
        }
        #endregion

        #region Instance Methods
        public override void Start()
        {
            Weapon weapon = thinkAction.Actor.EquippedWeapon;
            Script script;
            if (weapon == null || weapon.Data.Script == null)
                script = new Script("0.0 doDamage string:actor string:target>list>front bool:false");
            else
                script = weapon.Data.Script;
            scriptRunner = new ScriptRunner(script, Battle, thinkAction.Actor, thinkAction.Target);

            AnimationState animationState = thinkAction.Actor.BattleEntity.AnimationState;
            if (animationState.Animation.Duration <= 0.0f)
                scriptStartTime = 0.0f;
            else
                scriptStartTime = animationState.Animation.Duration - (animationState.Time % animationState.Animation.Duration);
        }

        public override void Finish()
        {
            base.Finish();
            if (!thinkAction.InfiniteInInventory)
                Battle.IncrementItemsUsed(Battle.GetPartyForPartyMember(thinkAction.Actor));
            PopState();
        }

        public override void Update(Delta delta)
        {
            scriptStartTimer += delta.Time;
            if (scriptStartTimer >= scriptStartTime)
            {
                scriptRunner.Update(delta);
                if (scriptRunner.IsFinished())
                    Finish();
            }
        }
        #endregion
    }
}
