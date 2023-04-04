using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public float foodDepletionRate { get; private set; }
    public float temperatureDepletionRate { get; private set; }
    public float sanityDepletionRate { get; private set; }


    [SerializeField] bool isServerHost;
    [SerializeField] NetworkManager networkManager;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
