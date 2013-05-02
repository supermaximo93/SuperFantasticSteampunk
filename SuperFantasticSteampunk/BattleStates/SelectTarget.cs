﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SuperFantasticSteampunk.BattleStates
{
    class SelectTarget : BattleState
    {
        #region Instance Fields
        private ThinkAction thinkAction;
        private List<PartyMember> potentialTargets;
        private int currentPotentialTargetIndex;
        private InputButtonListener inputButtonListener;
        #endregion

        #region Constructors
        public SelectTarget(Battle battle, ThinkAction thinkAction)
            : base(battle)
        {
            if (thinkAction == null)
                throw new Exception("ThinkAction cannot be null");
            this.thinkAction = thinkAction;
            potentialTargets = new List<PartyMember>();
            currentPotentialTargetIndex = 0;
            inputButtonListener = new InputButtonListener(new Dictionary<InputButton, ButtonEventHandlers> {
                { InputButton.Up, new ButtonEventHandlers(down: choosePreviousTarget) },
                { InputButton.Down, new ButtonEventHandlers(down: chooseNextTarget) },
                { InputButton.A, new ButtonEventHandlers(up: selectTarget) },
                { InputButton.Y, new ButtonEventHandlers(up: cancelTargetSelection) }
            });
        }
        #endregion

        #region Instance Methods
        public override void Start()
        {
            if (thinkAction.Type == ThinkActionType.Attack || thinkAction.Type == ThinkActionType.Defend)
            {
                WeaponData weaponData = ResourceManager.GetWeaponData(thinkAction.OptionName);
                if (weaponData != null)
                {
                    if (weaponData.WeaponUseAgainst == WeaponUseAgainst.Player || weaponData.WeaponUseAgainst == WeaponUseAgainst.Both)
                        potentialTargets.AddRange(Battle.PlayerParty);
                    if (weaponData.WeaponUseAgainst == WeaponUseAgainst.Enemy || weaponData.WeaponUseAgainst == WeaponUseAgainst.Both)
                        potentialTargets.AddRange(Battle.EnemyParty);
                }
            }
            else
                potentialTargets.AddRange(Battle.PlayerParty);
        }

        public override void Finish()
        {
            PopState();
        }

        public override void Update(GameTime gameTime)
        {
            if (potentialTargets.Count == 0)
            {
                Finish();
                return;
            }

            inputButtonListener.Update(gameTime);
        }

        private void choosePreviousTarget()
        {
            if (--currentPotentialTargetIndex < 0)
                currentPotentialTargetIndex = potentialTargets.Count - 1;
        }

        private void chooseNextTarget()
        {
            if (++currentPotentialTargetIndex >= potentialTargets.Count)
                currentPotentialTargetIndex = 0;
        }

        private void selectTarget()
        {

            thinkAction.Target = potentialTargets[currentPotentialTargetIndex];
            Finish();
        }

        private void cancelTargetSelection()
        {
            thinkAction.Target = null;
            Finish();
        }
        #endregion
    }
}
