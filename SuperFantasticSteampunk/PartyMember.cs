﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spine;
using SuperFantasticSteampunk.BattleStates;

namespace SuperFantasticSteampunk
{
    class PartyMember
    {
        #region Instance Fields
        private List<StatModifier> statModifiers;
        private List<StatusEffect> statusEffects;
        #endregion

        #region Instance Properties
        public PartyMemberData Data { get; private set; }

        public string Name { get; private set; }

        public int MaxHealth { get; private set; }
        public int Health { get; private set; }
        public int Speed { get; private set; }

        public Entity BattleEntity { get; private set; }
        public Entity OverworldEntity { get; private set; }

        public Weapon EquippedWeapon { get; private set; }
        public Shield EquippedShield { get; private set; }

        public bool HurtThisTurn { get; private set; }

        public bool Alive
        {
            get { return Health > 0; }
        }

        public CharacterClass CharacterClass
        {
            get { return Data.CharacterClass; }
        }
        #endregion

        #region Constructors
        public PartyMember(PartyMemberData partyMemberData)
        {
            if (partyMemberData == null)
                throw new Exception("PartyMemberData cannot be null");
            Data = partyMemberData;
            statModifiers = new List<StatModifier>();
            statusEffects = new List<StatusEffect>();
            BattleEntity = null;
            generateName();
            resetStats();
        }
        #endregion

        #region Instance Methods
        public void StartOverworld(Vector2 entityPosition)
        {
            OverworldEntity = new Entity(ResourceManager.GetNewSprite(Data.OverworldSpriteName), entityPosition);
        }

        public void FinishOverworld()
        {
            OverworldEntity = null;
        }
        
        public void StartBattle()
        {
            statModifiers.Clear();
            statusEffects.Clear();
            calculateStatsFromModifiers();

            BattleEntity = new Entity(ResourceManager.GetNewSkeleton(Data.BattleSkeletonName), new Vector2());
            BattleEntity.Skeleton.SetSkin(Data.BattleSkeletonSkinName);
            Animation animation = BattleEntity.Skeleton.Data.FindAnimation("idle");
            if (animation != null)
                BattleEntity.AnimationState.SetAnimation(animation, true);
            BattleEntity.Scale = new Vector2(0.6f);
            BattleEntity.AnimationState.Time = Game1.Random.Next(100) / 100.0f;
            BattleEntity.Altitude = Data.BattleAltitude;
            if (Data.BattleShadowFollowBoneName != null && Data.BattleShadowFollowBoneName.Length > 0)
                BattleEntity.ShadowFollowBone = BattleEntity.Skeleton.FindBone(Data.BattleShadowFollowBoneName);

            updateBattleEntitySkeleton();

            HurtThisTurn = false;
        }

        public void FinishBattle()
        {
            statModifiers.Clear();
            statusEffects.Clear();
            BattleEntity = null;
            HurtThisTurn = false;
        }

        public void EndTurn()
        {
            for (int i = statusEffects.Count - 1; i >= 0; --i)
            {
                if (!statusEffects[i].Active)
                    statusEffects.RemoveAt(i);
            }
            for (int i = statModifiers.Count - 1; i >= 0; --i)
            {
                statModifiers[i].DecrementTurnsLeft();
                if (!statModifiers[i].Active)
                    statModifiers.RemoveAt(i);
            }
            calculateStatsFromModifiers();
            HurtThisTurn = false;
        }

        public void Kill(Battle battle)
        {
            if (BattleEntity != null)
                BattleEntity.Kill();

            battle.GetPartyBattleLayoutForPartyMember(this).Try(pbl => pbl.RemovePartyMember(this));
            battle.GetPartyForPartyMember(this).Try(pbl => pbl.RemovePartyMember(this));
        }

        public void AddStatModifier(StatModifier statModifier)
        {
            statModifiers.Add(statModifier);
            calculateStatsFromModifiers();
        }

        public void AddStatusEffect(StatusEffect statusEffect)
        {
            if (!HasStatusEffect(statusEffect.Type))
                statusEffects.Add(statusEffect);
        }

        public void ForEachStatusEffect(Action<StatusEffect> action)
        {
            statusEffects.ForEach(action);
        }

        public bool HasStatusEffect(StatusEffectType statusEffectType)
        {
            foreach (StatusEffect statusEffect in statusEffects)
            {
                if (statusEffect.Type == statusEffectType)
                    return true;
            }
            return false;
        }

        public bool FearsPartyMember(PartyMember other)
        {
            foreach (StatusEffect statusEffect in statusEffects)
            {
                if (statusEffect.Type == StatusEffectType.Fear)
                {
                    if ((statusEffect as StatusEffects.Fear).Inflictor == other)
                        return true;
                }
            }
            return false;
        }

        public void EquipWeapon(string name)
        {
            EquippedShield = null;
            if (name == null)
                EquippedWeapon = null;
            else if (EquippedWeapon == null || EquippedWeapon.Data.Name != name)
                EquippedWeapon = ResourceManager.GetNewWeapon(name);
            updateBattleEntitySkeleton();
        }

        public void EquipDefaultWeapon(Party party)
        {
            foreach (KeyValuePair<string, int> inventoryItem in party.WeaponInventories[CharacterClass])
            {
                if (inventoryItem.Value < 0)
                {
                    EquipWeapon(inventoryItem.Key);
                    if (EquippedWeapon != null)
                        break;
                }
            }
        }

        public void EquipShield(string name)
        {
            EquippedWeapon = null;
            if (name == null)
                EquippedShield = null;
            else if (EquippedShield == null || EquippedShield.Data.Name != name)
                EquippedShield = ResourceManager.GetNewShield(name);
            updateBattleEntitySkeleton();
        }

        public void DoDamage(int amount, bool ignoreShield)
        {
            if (amount > 0 && !ignoreShield)
                HurtThisTurn = true;

            Health -= amount;
            if (Health < 0)
                Health = 0;
            else if (Health > MaxHealth)
                Health = MaxHealth;
        }

        public int CalculateDamageTaken(PartyMember enemy)
        {
            int damageToDo = enemy.calculateFinalAttackStat();
            int damageToBlock = calcuateFinalDefenceStat();
            int damage = damageToDo - damageToBlock;
            return damage < 0 ? 0 : damage;
        }

        private int calculateFinalAttackStat()
        {
            int result = 0;
            if (EquippedWeapon != null)
                result += EquippedWeapon.Data.Power;
            foreach (StatModifier statModifier in statModifiers)
                result += EquippedWeapon.Data.WeaponType == WeaponType.Ranged ? statModifier.RangedAttack : statModifier.MeleeAttack;
            return result;
        }

        private int calcuateFinalDefenceStat()
        {
            int result = 0;
            if (EquippedShield != null)
                result += EquippedShield.Data.Defence;
            foreach (StatModifier statModifier in statModifiers)
                result += statModifier.Defence;
            return result;
        }

        private void resetStats()
        {
            MaxHealth = Data.MaxHealth;
            Speed = Data.Speed;
            Health = MaxHealth;
        }

        private void calculateStatsFromModifiers()
        {
            MaxHealth = Data.MaxHealth;
            Speed = Data.Speed;

            foreach (StatModifier statModifier in statModifiers)
            {
                MaxHealth += (int)(Data.MaxHealth * statModifier.MaxHealth);
                Speed += (int)(Data.Speed * statModifier.Speed);
            }

            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        private void updateBattleEntitySkeleton()
        {
            if (BattleEntity == null)
                return;

            if (EquippedWeapon != null && EquippedWeapon.TextureData != null)
                BattleEntity.SetSkeletonAttachment("weapon", EquippedWeapon.Data.Name, EquippedWeapon.TextureData);
            else
                BattleEntity.SetSkeletonAttachment("weapon", "none", forceNoTextureData: true);

            if (EquippedShield != null && EquippedShield.TextureData != null)
                BattleEntity.SetSkeletonAttachment("shield", EquippedShield.Data.Name, EquippedShield.TextureData);
            else
                BattleEntity.SetSkeletonAttachment("shield", "none", forceNoTextureData: true);
        }

        private void generateName()
        {
            if (Data.CharacterClass == CharacterClass.Enemy)
                Name = Data.Name;
            else
                Name = ResourceManager.PartyMemberTitles.Sample() + " " + ResourceManager.PartyMemberForenames.Sample() + " " + ResourceManager.PartyMemberSurnames.Sample();
        }
        #endregion
    }
}
