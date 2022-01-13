using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowersManager : MonoBehaviour
{
    public static PowersManager instance;
    public List<Power> powers;

    private void Awake()
    {
        if(instance != null) {
            Debug.LogWarning("Multiple instances of PowerupsManager found!");
            return;
        }
        instance = this;
    }

    public bool IsPowerActive(string powerName)
    {
        foreach(Power power in powers) {
            if(power.name == powerName) {
                return power.isActive;
            }
        }

        // Power not found, return false
        return false;
    }

    public void ActivatePower(string powerName)
    {
        foreach(Power power in powers) {
            if(power.name == powerName) {
                power.isActive = true;
            }
        }
    }

    public void DeactivatePower(string powerName)
    {
        foreach(Power power in powers) {
            if(power.name == powerName) {
                power.isActive = false;
            }
        }
    }
}
