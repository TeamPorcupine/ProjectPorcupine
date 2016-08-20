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
            if (_currentHealth == 0 && value > _currentHealth)
            {
                _currentHealth = value;

                if (OnRevive != null)
                    OnRevive();
                return;
            }
            //we have been hit but we are not dead!
            if (_currentHealth <= 0 && value > 0)
            {
                _currentHealth = value;
                if (OnHit != null)
                    OnHit();
            }
            //we have been healed!
            if (value > _currentHealth)
            {
                if (value > MaxHealth)
                    value = MaxHealth;
                _currentHealth = value;
                if (OnHeal != null)
                    OnHeal();

            }
            //We have died and this means we have been hit!
            if (value <= 0 && IsAlive)
            {
                _currentHealth = 0;
                if (OnHit != null)
                    OnHit();
                if (OnDestruction != null)
                    OnDestruction();
            }
        }
    }
    public float MaxHealth { get; set; }
    public bool Invincible { get; set; }
    public bool Healable { get; set; }
    public bool IsAlive { get { return Value > 0; } }

    public event Action OnHit;
    public event Action OnHeal;
    public event Action OnRevive;
    public event Action OnDestruction;

    private float _currentHealth = 100.0f;


    //Just to have a default constructor so everything stays as default value.
    public HealthSystem() { }

    public HealthSystem(float currentHealth, float maxHealth, bool invincible, bool healAble)
    {
        _currentHealth = currentHealth;
        MaxHealth = maxHealth;
        Invincible = invincible;
        Healable = healAble;
    }

    private HealthSystem(HealthSystem h)
    {
        Value = h.Value;
        MaxHealth = h.MaxHealth;
        Invincible = h.Invincible;
        Healable = h.Healable;
        _currentHealth = h._currentHealth;
        OnHit = h.OnHit;
        OnHeal = h.OnHeal;
        OnRevive = h.OnRevive;
        OnDestruction = h.OnDestruction;
    }

    public static HealthSystem operator+(HealthSystem h1, float num)
    {
        HealthSystem h = new HealthSystem(h1);
        h.Value += num;
        return h;
    }
    public static HealthSystem operator -(HealthSystem h1, float num)
    {
        HealthSystem h = new HealthSystem(h1);
        h.Value -= num;
        return h;
    }
}
