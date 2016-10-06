using UnityEngine;
using System.Collections;

public class Health {
    public enum ResistanceType
    {
        fire,
        laser,
        explosive,
        melee
    }

    public void DamageEntity(Object temp, ResistanceType resistance, int damageAmount)
    {
        Furniture furn = new Furniture();
        if (temp.GetType().Equals(furn.GetType()))
        {
            Debug.ULogChannel("Health.cs", "This is a Furniture object.");
        }
    }

    public void DamageCharacter(GameObject go, ResistanceType resistance, int damageAmount)
    {
        
    }

}
