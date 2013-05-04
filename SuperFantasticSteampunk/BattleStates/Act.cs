﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SuperFantasticSteampunk.BattleStates
{
    class Act : BattleState
    {
        #region Instance Fields
        private List<ThinkAction> attackThinkActions;
        private List<ThinkAction> defendThinkActions;
        private List<ThinkAction> useItemThinkActions;
        #endregion

        #region Constructors
        public Act(Battle battle, List<ThinkAction> thinkActions)
            : base(battle)
        {
            if (thinkActions == null)
                throw new Exception("List<ThinkAction> cannot be null");

            attackThinkActions = new List<ThinkAction>(thinkActions.Where(thinkAction => thinkAction.Type == ThinkActionType.Attack));
            defendThinkActions = new List<ThinkAction>(thinkActions.Where(thinkAction => thinkAction.Type == ThinkActionType.Defend));
            useItemThinkActions = new List<ThinkAction>(thinkActions.Where(thinkAction => thinkAction.Type == ThinkActionType.UseItem));
        }
        #endregion

        #region Instance Methods
        public override void Start()
        {
            Comparison<ThinkAction> speedComparer = new Comparison<ThinkAction>((a, b) => b.Actor.Speed.CompareTo(a.Actor.Speed));
            attackThinkActions.Sort(speedComparer);
            defendThinkActions.Sort(speedComparer);
            useItemThinkActions.Sort(speedComparer);
        }

        public override void Finish()
        {
            ChangeState(new EndTurn(Battle));
        }

        public override void Update(GameTime gameTime)
        {
            //TODO: Change these to individual states
            foreach (ThinkAction thinkAction in defendThinkActions)
            {
                thinkAction.Actor.EquipShield(thinkAction.OptionName);
                Logger.Log(thinkAction.Actor.Data.Name + " equipped '" + thinkAction.OptionName + "' shield");
            }

            foreach (ThinkAction thinkAction in useItemThinkActions)
            {
                Logger.Log(thinkAction.Actor.Data.Name + " used '" + thinkAction.OptionName + "' item");
                Logger.Log("TODO: use item"); //TODO: use item
            }

            foreach (ThinkAction thinkAction in attackThinkActions)
            {
                thinkAction.Actor.EquipWeapon(thinkAction.OptionName);
                Logger.Log(thinkAction.Actor.Data.Name + " equipped '" + thinkAction.OptionName + "' weapon");

                if (thinkAction.Target.Alive)
                {
                    int damage = thinkAction.Target.CalculateDamageTaken(thinkAction.Actor);
                    thinkAction.Target.DoDamage(damage);
                    Logger.Log(thinkAction.Actor.Data.Name + " did " + damage.ToString() + " damage to " + thinkAction.Target.Data.Name);
                }

                if (!thinkAction.Target.Alive)
                {
                    Logger.Log(thinkAction.Actor.Data.Name + " target " + thinkAction.Target.Data.Name + " is not alive");
                    thinkAction.Target.Kill(Battle);
                    thinkAction.Target = null;
                    //TODO: choose new target
                }
            }

            Finish();
        }
        #endregion
    }
}
