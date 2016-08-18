using UnityEngine;
using System.Collections;

public interface IDamageable {

	void takeDemage (int amount);

	bool heal (int amount);

	int getHitPoints ();

	int getMaximumHitPoints ();
}
