using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    
    private bool inputIsFree = true;
    public bool InputIsFree
    {
        get { return inputIsFree; }
    }

    private void Awake()
    {
        #region Singleton

        if(instance != null) {
            Debug.LogWarning("Multiple instances of InputManager found!");
            return;
        }
        instance = this;

        #endregion
    }

    public void BlockInput()
    {
        if(!inputIsFree) {
            Debug.LogWarning("Input is already blocked!");
        }
        inputIsFree = false;
    }

    public void UnblockInput()
    {
        if(inputIsFree) {
            Debug.LogWarning("Input is already free!");
        }
        inputIsFree = true;
    }
}
