﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spine;

namespace SuperFantasticSteampunk
{
    class PartyMember
    {
        #region Constants
        public int MaxStat = 250;
        public int MaxLevel = 99;
        public int ExperienceNeededToLevelUp = 100;
        private double experienceConstant = 10.0;
        #endregion

        #region Instance Fields
        private List<StatModifier> statModifiers;
        #endregion

        #region Instance Properties
        public PartyMemberData Data { get; private set; }

        public int BaseMaxHealth { get; private set; }
        public int BaseAttack { get; private set; }
        public int BaseSpecialAttack { get; private set; }
        public int BaseDefence { get; private set; }
        public int BaseSpeed { get; private set; }
        public int BaseCharm { get; private set; }

        public int MaxHealth { get; private set; }
        public int Health { get; private set; }
        public int Attack { get; private set; }
        public int SpecialAttack { get; private set; }
        public int Defence { get; private set; }
        public int Speed { get; private set; }
        public int Charm { get; private set; }
        public int Level { get; private set; }
        public int Experience { get; private set; }
        public Entity BattleEntity { get; private set; }

        public Weapon EquippedWeapon { get; private set; }
        public Shield EquippedShield { get; private set; }

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
            statModifiers = new List<StatModifier>();
            Data = partyMemberData;
            BattleEntity = null;
            Level = 50;
            resetBaseStatsFromLevel();
        }
        #endregion

        #region Instance Methods
        public void StartBattle()
        {
            statModifiers.Clear();
            resetBaseStatsFromLevel();
            calculateStatsFromModifiers();

            BattleEntity = new Entity(ResourceManager.GetNewSkeleton(Data.SkeletonName), new Vector2());
            BattleEntity.Skeleton.SetSkin(Data.SkeletonSkinName);
            Animation animation = BattleEntity.Skeleton.Data.FindAnimation("idle");
            if (animation != null)
                BattleEntity.AnimationState.SetAnimation(animation, true);
            BattleEntity.Scale = new Vector2(0.6f);
            BattleEntity.AnimationState.Time = Game1.Random.Next(100) / 100.0f;
        }

        public void FinishBattle()
        {
            statModifiers.Clear();
            BattleEntity = null;
            EquippedWeapon = null;
            EquippedShield = null;
        }

        public void EndTurn()
        {
            for (int i = statModifiers.Count - 1; i >= 0; --i)
            {
                statModifiers[i].DecrementTurnsLeft();
                if (!statModifiers[i].Active)
                    statModifiers.RemoveAt(i);
            }
            calculateStatsFromModifiers();
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

        public void EquipWeapon(string name)
        {
            EquippedShield = null;
            if (EquippedWeapon == null || EquippedWeapon.Data.Name != name)
                EquippedWeapon = ResourceManager.GetNewWeapon(name);
            updateBattleEntitySkeleton();
        }

        public void EquipShield(string name)
        {
            EquippedWeapon = null;
            if (EquippedShield == null || EquippedShield.Data.Name != name)
                EquippedShield = ResourceManager.GetNewShield(name);
            updateBattleEntitySkeleton();
        }

        public void DoDamage(int amount)
        {
            Health -= amount;
            if (Health < 0)
                Health = 0;
            else if (Health > MaxHealth)
                Health = MaxHealth;
        }

        public int CalculateDamageTaken(PartyMember enemy)
        {
            int damageToDo = enemy.calculateFinalAttackStat() * (enemy.criticalHit() ? 2 : 1);
            int damageToBlock = calcuateFinalDefenceStat();
            int damage = damageToDo - damageToBlock;
            return damage <= 0 ? 1 : damage;
        }

        public void AddExperience(PartyMember enemy)
        {
            addExperience(calculateExperienceGained(enemy.Level, enemy.Data.ExperienceMultiplier));
        }

        private void addExperience(int amount)
        {
            Experience += amount;
            while (Experience >= ExperienceNeededToLevelUp)
            {
                ++Level;
                Experience -= ExperienceNeededToLevelUp;
            }
            if (Level > MaxLevel)
            {
                Level = MaxLevel;
                Experience = ExperienceNeededToLevelUp;
            }
            resetBaseStatsFromLevel();
            calculateStatsFromModifiers();
        }

        private int calculateExperienceGained(int enemyLevel, int enemyExperienceMultiplier)
        {
            return (int)Math.Round((enemyLevel / (double)Level) * enemyExperienceMultiplier * experienceConstant);
        }

        private int calculateFinalAttackStat()
        {
            int result = 0;
            if (EquippedWeapon == null)
                result = Attack;
            else
            {
                result += EquippedWeapon.Data.Power;
                if (EquippedWeapon.Data.WeaponType == WeaponType.Melee)
                    result += Attack;
                else
                    result += SpecialAttack;
            }
            return result;
        }

        private int calcuateFinalDefenceStat()
        {
            int result = Defence;
            if (EquippedShield != null)
                result += EquippedShield.Data.Defence;
            return result;
        }

        private bool criticalHit()
        {
            return Game1.Random.Next(MaxStat) < Charm;
        }

        private void calculateStatsFromModifiers()
        {
            MaxHealth = BaseMaxHealth;
            Attack = BaseAttack;
            SpecialAttack = BaseSpecialAttack;
            Defence = BaseDefence;
            Speed = BaseSpeed;
            Charm = BaseCharm;

            foreach (StatModifier statModifier in statModifiers)
            {
                MaxHealth += (int)(BaseMaxHealth * statModifier.MaxHealth);
                Attack += (int)(BaseAttack * statModifier.Attack);
                SpecialAttack += (int)(BaseSpecialAttack * statModifier.SpecialAttack);
                Defence += (int)(BaseDefence * statModifier.Defence);
                Speed += (int)(BaseSpeed * statModifier.Speed);
                Charm += (int)(BaseCharm * statModifier.Charm);
            }

            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        private void resetBaseStatsFromLevel()
        {
            BaseMaxHealth = (int)Math.Round(Level * (Data.MaxHealth / (double)MaxLevel));
            BaseAttack = (int)Math.Round(Level * (Data.Attack / (double)MaxLevel));
            BaseSpecialAttack = (int)Math.Round(Level * (Data.SpecialAttack / (double)MaxLevel));
            BaseDefence = (int)Math.Round(Level * (Data.Defence / (double)MaxLevel));
            BaseSpeed = (int)Math.Round(Level * (Data.Speed / (double)MaxLevel));
            BaseCharm = (int)Math.Round(Level * (Data.Charm / (double)MaxLevel));
            Health = BaseMaxHealth;
        }

        private void updateBattleEntitySkeleton()
        {
            if (BattleEntity == null)
                return;

            if (EquippedWeapon != null && EquippedWeapon.TextureData != null)
                BattleEntity.SetSkeletonAttachment("weapon", EquippedWeapon.Data.Name, EquippedWeapon.TextureData);
            else
                BattleEntity.SetSkeletonAttachment("weapon", "none");

            if (EquippedShield != null && EquippedShield.TextureData != null)
                BattleEntity.SetSkeletonAttachment("shield", EquippedShield.Data.Name, EquippedShield.TextureData);
            else
                BattleEntity.SetSkeletonAttachment("shield", "none");
        }
        #endregion
    }
}
