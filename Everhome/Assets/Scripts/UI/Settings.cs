using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Settings : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction leftArmAction;
    private InputAction rightArmAction;

    void Start()
    {
        jumpAction = InputSystem.actions.FindAction("Jump");
        moveAction = InputSystem.actions.FindAction("Move");
        leftArmAction = InputSystem.actions.FindAction("Lefthand");
        rightArmAction = InputSystem.actions.FindAction("Righthand");
    }

    public void UpdateBinding(string name, string bind)
    {
        InputAction action = InputSystem.actions.FindAction("name");

        int index;
        if(bind != "") index = action.bindings.IndexOf(b => b.name == "down");
        else index = action.bindings.IndexOf(b => !b.isComposite && !b.isPartOfComposite);

        action.PerformInteractiveRebinding(index).Start();
    }

    //action.actionMap.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds")); -- load
    //PlayerPrefs.SetString("rebinds", action.actionMap.SaveBindingOverridesAsJson()); -- save
}
