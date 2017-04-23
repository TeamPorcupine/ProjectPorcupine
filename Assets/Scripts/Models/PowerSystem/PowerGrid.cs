#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Collections.Generic;
using System.Linq;

public class PowerGrid
{
	private HashSet<PowerRelated> powerGrid;
	
	public bool IsOperating { get; private set; }
	
	public PowerGrid()
	{
		powerGrid = new HashSet<PowerRelated>();
	}
	
	public bool CanPlugIn(PowerRelated powerRelated)
	{
		if(powerRelated == null) throw new ArgumentNullException("powerRelated");
		return true;
	}
	
	public bool PlugIn(PowerRelated powerRelated)
	{
		if(powerRelated == null) throw new ArgumentNullException("powerRelated");
		if(!CanPlugIn(powerRelated))
		{
			return false;
		}
		
		powerGrid.Add(powerRelated);
		return true;
	}
	
	public bool IsPluggedIn(PowerRelated powerRelated)
	{
		if(powerRelated == null) throw new ArgumentNullException("powerRelated");
		return powerGrid.Contein(powerRelated);
	}
	
	public void Unplug(PowerRelated powerRelated)
	{
		if(powerRelated == null) throw new ArgumentNullException("powerRelated");
		powerGrid.Remove(powerRelated);
	}
	
	public bool Update()
	{
		float currentPowerLevel = 0.0f;
		foreach(PowerRelated powerRelated in powerGrid.Where(powerRelated => powerRelated.IsPowerProducer))
		{
			currentPowerLevel += powerRelated.OutputRate;
		}
		
		foreach(PowerRelated powerRelated in powerGrid.Where(powerRelated => powerRelated.IsPowerConsumer))
		{
			currentPowerLevel -= powerRelated.InputRate;
		}
		
		if(currentPowerLevel > 0.0f)
		{
			ChargeAccumulators(ref currentPowerLevel);
		} else
		{
			DischargeAccumulators(ref currentPowerLevel);
		}
		
		IsOperating = currentPowerLevel >= 0.0f;
	}
	
	private void ChargeAccumulators(ref float currentPowerLevel)
	{
		foreach(PowerRelated powerRelated in powerGrid.Where(powerRelated => powerRelated.IsPowerAccumulator && !powerRelated.IsFull))
		{
			if(currentPowerLevel - powerRelated.InputRate < 0.0f) break;
			currentPowerLevel -= powerRelated.InputRate;
			powerRelated.AccumulatedPower += powerRelated.InputRate;
		}
	}
	
	private void DischargeAccumulators(ref float currentPowerLevel)
	{
		foreach(PowerRelated powerRelated in powerGrid.Where(powerRelated => powerRelated.IsPowerAccumulator && !powerRelated.IsEmpty))
		{
			currentPowerLevel += powerRelated.OutputRate;
			powerRelated.AccumulatedPower -= powerRelated.OutputRate;
			if(currentPowerLevel >= 0.0f) break;
		}
	}
}
