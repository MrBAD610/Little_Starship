using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
    public Transform playerLocation;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
        agent.destination = playerLocation.position;
    }
}
