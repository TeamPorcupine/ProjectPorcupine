#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;

[Serializable]
public class Health
{
    //// Health States Order
    //// Revive, Heal, Overheal, Hit, Destroyed

    private float healthValue;

    public Health(float maxHealth)
    {
        MaxHealth = maxHealth;
        healthValue = maxHealth;
    }

    public Health(float maxHealth, bool isInvincible, bool isRevivable, bool canOveheal)
    {
        MaxHealth = maxHealth;
        healthValue = maxHealth;
        IsInvincible = isInvincible;
        IsRevivable = isRevivable;
        CanOverheal = canOveheal;
    }

    private Health(Health other)
    {
        healthValue = other.healthValue;
        MaxHealth = other.MaxHealth;
        IsInvincible = other.IsInvincible;
        IsRevivable = other.IsRevivable;
        CanOverheal = other.CanOverheal;
    }

    public event Action Revive;

    public event Action Heal;

    public event Action Overheal;

    public event Action Hit;

    public event Action Destroyed;

    public float Value
    {
        get
        {
            return healthValue;
        }

        set
        {
            if (value > healthValue)
            {
                float remainder = 0;
                bool isRevive = false;
                if (value > MaxHealth)
                {
                    // Get the remainder of our overhealed health for later
                    remainder = value - MaxHealth;
                    value -= remainder;
                }

                if (IsRevivable && healthValue == 0 && value > 0)
                {
                    isRevive = true;
                    healthValue = value;

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
                    healthValue = value;

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
                        healthValue += remainder;
                    }

                    // We have been overhealed whether or not we can apply the amount
                    if (Overheal != null)
                    {
                        Overheal();
                    }
                }

                return;
            }

            // Even if health is at 0, we should still be able to get hit for effects
            bool isDead = false;
            if (value < healthValue)
            {
                // Only apply health damage if the current Value is greater then 0
                if (!IsInvincible && healthValue > 0)
                {
                    if (value <= 0)
                    {
                        value = 0;
                        isDead = true;
                    }

                    healthValue = value;
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
    }

    public bool IsAlive
    {
        get { return healthValue > 0; }
    }

    public bool IsInvincible { get; set; }

    public bool IsRevivable { get; set; }

    public bool CanOverheal { get; set; }

    public float MaxHealth { get; set; }

    public float OverhealAmount
    {
        get
        {
            return healthValue - MaxHealth;
        }
    }

    public Health Clone()
    {
        return new Health(this);
    }
}