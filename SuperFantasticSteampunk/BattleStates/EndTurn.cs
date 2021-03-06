﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SuperFantasticSteampunk.BattleStates
{
    class EndTurn : BattleState
    {
        #region Constants
        private const float clockHourTransistionTimeInSeconds = 2.0f;
        #endregion

        #region Instance Fields
        Party currentStatusEffectParty;
        int currentStatusEffectPartyMemberIndex;
        float clockTime;
        #endregion

        #region Constructors
        public EndTurn(Battle battle)
            : base(battle)
        {
        }
        #endregion

        #region Instance Methods
        public override void Start()
        {
            currentStatusEffectParty = Battle.PlayerParty;
            currentStatusEffectPartyMemberIndex = 0;
            clockTime = 0.0f;
        }

        public override void Finish()
        {
            base.Finish();
            handlePartyFinish(Battle.PlayerParty);
            handlePartyFinish(Battle.EnemyParty);
            removeDeadPartyMembers();

            if (Battle.PlayerParty.Count == 0)
                ChangeState(new Lose(Battle));
            else if (Battle.EnemyParty.Count == 0)
                ChangeState(new Win(Battle));
            else
                ChangeState(new Think(Battle));
        }

        public override void Resume(BattleState previousBattleState)
        {
            base.Resume(previousBattleState);
            if (previousBattleState is HandleStatusEffects)
                ++currentStatusEffectPartyMemberIndex;
        }

        public override void Update(Delta delta)
        {
            if (currentStatusEffectPartyMemberIndex < currentStatusEffectParty.Count)
            {
                if (currentStatusEffectParty[currentStatusEffectPartyMemberIndex].Alive)
                    PushState(new HandleStatusEffects(Battle, StatusEffectEvent.EndTurn, partyMember: currentStatusEffectParty[currentStatusEffectPartyMemberIndex]));
                else
                    ++currentStatusEffectPartyMemberIndex;
            }
            else if (currentStatusEffectParty == Battle.PlayerParty)
            {
                currentStatusEffectParty = Battle.EnemyParty;
                currentStatusEffectPartyMemberIndex = 0;
            }
            else if (clockTime < clockHourTransistionTimeInSeconds)
            {
                clockTime += delta.Time;
                Clock.Update(delta.Time / clockHourTransistionTimeInSeconds);
            }
            else
                Finish();
        }

        private void handlePartyFinish(Party party)
        {
            foreach (PartyMember partyMember in party)
            {
                if (partyMember.EquippedShield != null)
                {
                    if (partyMember.HurtThisTurn)
                        Battle.IncrementItemsUsed(party);
                    else
                        party.ShieldInventory.AddItem(partyMember.EquippedShield.GetFullName(), partyMember);
                }
                partyMember.EndTurn();
                partyMember.BattleEntity.ResetManipulation("Scale");
            }
        }
        #endregion
    }
}
