using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyedStateController : MonoBehaviour
{
    public event Action<string, Animator, AnimatorStateInfo, int> onStateEnter;
    public event Action<string, Animator, AnimatorStateInfo, int> onStateUpdate;
    public event Action<string, Animator, AnimatorStateInfo, int> onStateExit;

    private readonly Dictionary<string, List<Action<string, Animator, AnimatorStateInfo, int>>> _stateEnterCallbacks =
        new Dictionary<string, List<Action<string, Animator, AnimatorStateInfo, int>>>();

    private readonly Dictionary<string, List<Action<string, Animator, AnimatorStateInfo, int>>> _stateUpdateCallbacks =
        new Dictionary<string, List<Action<string, Animator, AnimatorStateInfo, int>>>();

    private readonly Dictionary<string, List<Action<string, Animator, AnimatorStateInfo, int>>> _stateExitCallbacks =
        new Dictionary<string, List<Action<string, Animator, AnimatorStateInfo, int>>>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void __OnStateEnter(string key, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        onStateEnter?.Invoke(key, animator, stateInfo, layerIndex);
        InvokeCallbacksSafely(_stateEnterCallbacks, key, animator, stateInfo, layerIndex);
    }

    public void __OnStateUpdate(string key, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        onStateUpdate?.Invoke(key, animator, stateInfo, layerIndex);
        InvokeCallbacksSafely(_stateUpdateCallbacks, key, animator, stateInfo, layerIndex);
    }

    public void __OnStateExit(string key, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        onStateExit?.Invoke(key, animator, stateInfo, layerIndex);
        InvokeCallbacksSafely(_stateExitCallbacks, key, animator, stateInfo, layerIndex);
    }

    private void InvokeCallbacksSafely(
        Dictionary<string, List<Action<string, Animator, AnimatorStateInfo, int>>> callbackDictionary, string key,
        Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (callbackDictionary.ContainsKey(key))
        {
            callbackDictionary[key].ForEach(action => action?.Invoke(key, animator, stateInfo, layerIndex));
        }
    }

    public void AddStateEnterListener(string key, Action<string, Animator, AnimatorStateInfo, int> callback)
    {
        AddStateListener(_stateEnterCallbacks, key, callback);
    }

    public void AddStateUpdateListener(string key, Action<string, Animator, AnimatorStateInfo, int> callback)
    {
        AddStateListener(_stateUpdateCallbacks, key, callback);
    }

    public void AddStateExitListener(string key, Action<string, Animator, AnimatorStateInfo, int> callback)
    {
        AddStateListener(_stateExitCallbacks, key, callback);
    }

    private void AddStateListener(
        Dictionary<string, List<Action<string, Animator, AnimatorStateInfo, int>>> callbackDictionary, string key,
        Action<string, Animator, AnimatorStateInfo, int> callback)
    {
        if (!callbackDictionary.ContainsKey(key))
        {
            callbackDictionary[key] = new List<Action<string, Animator, AnimatorStateInfo, int>>();
        }

        callbackDictionary[key].Add(callback);
    }

    public void RemoveStateEnterListener(string key, Action<string, Animator, AnimatorStateInfo, int> callback)
    {
        RemoveStateListener(_stateEnterCallbacks, key, callback);
    }

    public void RemoveStateUpdateListener(string key, Action<string, Animator, AnimatorStateInfo, int> callback)
    {
        RemoveStateListener(_stateUpdateCallbacks, key, callback);
    }

    public void RemoveStateExitListener(string key, Action<string, Animator, AnimatorStateInfo, int> callback)
    {
        RemoveStateListener(_stateExitCallbacks, key, callback);
    }

    private void RemoveStateListener(
        Dictionary<string, List<Action<string, Animator, AnimatorStateInfo, int>>> callbackDictionary, string key,
        Action<string, Animator, AnimatorStateInfo, int> callback)
    {
        if (!callbackDictionary.ContainsKey(key))
        {
            return;
        }

        callbackDictionary[key].Remove(callback);
    }
}