using System;

[Serializable]
public class HealthSystem
{
    //all of this data should probably be generated at runtime or loaded from a file
    public float Value
    {
        get
        {
            return _currentHealth;
        }

        private set
        {
            if (Invincible == true) return;
            //We have been Revived!
            if (_currentHealth <= 0 && value > _currentHealth)
            {
                _currentHealth = value;

                if (_OnRevive != null)
                    _OnRevive();
                return;
            }
            //we have been hit but we are not dead!
            if (value < _currentHealth && value > 0)
            {
                _currentHealth = value;
                if (_OnHit != null)
                    _OnHit();
            }
            //we have been healed!
            if (value > _currentHealth)
            {
                if (value > MaxHealth)
                    value = MaxHealth;
                _currentHealth = value;
                if (_OnHeal != null)
                    _OnHeal();

            }
            //We have died and this means we have been hit!
            if (value <= 0 && IsAlive)
            {
                _currentHealth = 0;
                if (_OnHit != null)
                    _OnHit();
                if (_OnDestruction != null)
                    _OnDestruction();
            }
        }
    }
    public float MaxHealth { get; set; }
    public bool Invincible { get; set; }
    public bool Healable { get; set; }
    public bool IsAlive { get { return Value > 0; } }

    private float _currentHealth = 100.0f;
    private Action _OnHit;
    private Action _OnHeal;
    private Action _OnRevive;
    private Action _OnDestruction;

    public HealthSystem(float currentHealth, float maxHealth, bool invincible, bool healAble)
    {
        _currentHealth = currentHealth;
        MaxHealth = maxHealth;
        Invincible = invincible;
        Healable = healAble;
    }

    public void RegisterOnHit(Action act)
    {
        _OnHit += act;
    }
    public void RegisterOnHeal(Action act)
    {
        _OnHeal += act;
    }
    public void RegisterOnRevive(Action act)
    {
        _OnRevive += act;
    }
    public void RegisterOnDestruction(Action act)
    {
        _OnDestruction += act;
    }

    public void UnregisterOnHit(Action act)
    {
        _OnHit -= act;
    }
    public void UnregisterOnHeal(Action act)
    {
        _OnHeal -= act;
    }
    public void UnregisterOnRevive(Action act)
    {
        _OnRevive -= act;
    }
    public void UnregisterOnDestruction(Action act)
    {
        _OnDestruction -= act;
    }

    public static HealthSystem operator+(HealthSystem h1, float num)
    {
        h1.Value += num;
        return h1;
    }
    public static HealthSystem operator -(HealthSystem h1, float num)
    {
        h1.Value -= num;
        return h1;
    }
}
