using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    private float inputCooldownCounter;
    public bool inputIsFree = true;

    private void Awake()
    {
        if(instance != null) {
            Debug.LogWarning("Multiple instances of InputManager found!");
            return;
        }
        instance = this;
    }

    private void Update()
    {
        // Prevent new inputs
        if(inputCooldownCounter > 0) {
            inputCooldownCounter -= Time.deltaTime;
        }
        // If inputCooldownCounter <= 0 and new inputs are blocked, unblock them
        else if(!inputIsFree) {
            inputIsFree = true;
        }
    }

    public void BlockInput(float inputCooldown = .1f)
    {
        inputIsFree = false;
        inputCooldownCounter = inputCooldown;
    }
}
