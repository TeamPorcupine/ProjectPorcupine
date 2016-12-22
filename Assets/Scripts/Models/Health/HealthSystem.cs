#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using MoonSharp.Interpreter;
using ProjectPorcupine.Localization;

[MoonSharpUserData]
[Serializable]
public class HealthSystem
{
    //// Health States Order
    //// Revive, Heal, Overheal, Hit, Destroyed

    private float currentHealth;

    public HealthSystem(float maxHealth)
    {
        MaxHealth = maxHealth;
        currentHealth = maxHealth;
    }

    public HealthSystem(float maxHealth, bool isInvincible, bool isHealable, bool isRevivable, bool canOveheal)
    {
        MaxHealth = maxHealth;
        currentHealth = maxHealth;
        IsInvincible = isInvincible;
        IsHealable = isHealable;
        IsRevivable = isRevivable;
        CanOverheal = canOveheal;
    }

    private HealthSystem(HealthSystem other)
    {
        currentHealth = other.currentHealth;
        MaxHealth = other.MaxHealth;
        IsInvincible = other.IsInvincible;
        IsHealable = other.IsHealable;
        IsRevivable = other.IsRevivable;
        CanOverheal = other.CanOverheal;
    }

    public event Action Revive;

    public event Action Heal;

    // TODO: Will be used to notify user if healing using something like a medkit will heal for X amount with some wasted (because we hit maxHP)
    public event Action Overheal;

    public event Action Hit;

    public event Action Destroyed;

    public float CurrentHealth
    {
        get
        {
            return currentHealth;
        }

        set
        {
            // New health greater than current health.
            if (value > currentHealth)
            {
                EvaluateHealthIncrease(value);
            }

            // New health less than current health.
            if (value < currentHealth)
            {
                EvaluateHealthDecrease(value);
            }
        }
    }

    /// <summary>
    /// Used to determine if the entity is still alive.
    /// </summary>
    public bool IsAlive
    {
        get { return currentHealth > 0; }
    }

    public bool IsInvincible { get; set; }

    public bool IsHealable { get; set; }

    public bool IsRevivable { get; set; }

    public bool CanOverheal { get; set; }

    public float MaxHealth { get; set; }

    public float OverhealAmount
    {
        get
        {
            return currentHealth - MaxHealth;
        }
    }

    public HealthSystem Clone()
    {
        return new HealthSystem(this);
    }

    /// <summary>
    /// Applies damage to the entity.
    /// </summary>
    /// <param name="damage">The number of hitpoints to remove from the entity.</param>
    /// TODO: Add calculations with resistance.
    /// TODO: Move to some sort of combat folder/class maybe the LUA?
    public void DamageEntity(float damage)
    {
        CurrentHealth -= damage;
    }

    /// <summary>
    /// Formats the text for health to be displayed by the Selection Panel.
    /// </summary>
    /// <returns>The newly formatted text.</returns>
    public string TextForSelectionPanel()
    {
        if (MaxHealth > 0)
        {
            return LocalizationTable.GetLocalization("hit_points_fraction", currentHealth, MaxHealth);
        }
        else
        {
            return LocalizationTable.GetLocalization("hit_points", "not_applicable_shorthand");
        }  
    }

    private void EvaluateHealthIncrease(float value)
    {
        float remainder = 0;
        bool isRevive = false;
        if (value > MaxHealth)
        {
            // Get the remainder of our overhealed health for later
            remainder = value - MaxHealth;
            value -= remainder;
        }

        if (IsRevivable && currentHealth == 0 && value > 0)
        {
            isRevive = true;
            currentHealth = value;

            // Revive
            if (Revive != null)
            {
                Revive();
            }

            // We just got revived and technically we also just got healed
            if (Heal != null)
            {
                Heal();
            }
        }

        if (IsAlive && !isRevive)
        {
            currentHealth = value;

            // Heal
            if (Heal != null)
            {
                Heal();
            }
        }

        // Overheal
        if (IsAlive && remainder > 0)
        {
            // If we can be overhealed then apply the amount of overheal
            if (CanOverheal)
            {
                currentHealth += remainder;
            }

            // We have been overhealed whether or not we can apply the amount
            if (Overheal != null)
            {
                Overheal();
            }
        }
    }

    private void EvaluateHealthDecrease(float value)
    {
        // Even if health is at 0, we should still be able to get hit for effects
        bool isDead = false;

        // Only apply health damage if the current Value is greater then 0
        if (!IsInvincible && currentHealth > 0)
        {
            if (value <= 0)
            {
                value = 0;
                isDead = true;
            }

            currentHealth = value;
        }

        if (Hit != null)
        {
            Hit();
        }

        if (isDead)
        {
            // Destroyed
            if (Destroyed != null)
            {
                Destroyed();
            }
        }
    }
}
