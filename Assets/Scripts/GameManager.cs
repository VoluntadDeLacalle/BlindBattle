using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public const int MaxPlayersPerTeam = 5;

    private new void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
