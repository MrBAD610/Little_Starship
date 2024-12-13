using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBrain : MonoBehaviour
{
    private GameObject player;
    private Transform playerLocation;
    private NavMeshAgent agent;
    private Transform agentLocation;

    private EnemyReferences enemyReferences;

    [Header("Stats")]
    public float pathUpdateDelay = 0.2f;
    public float detectionRadius = 5f;

    [Header("References")]
    [SerializeReference] private GameObject DeadEnemy;

    private float pathUpdateDeadline;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool hasAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        enemyReferences = GameObject.Find("Enemy References").GetComponent<EnemyReferences>();
        player = enemyReferences.playerReference;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        playerLocation = enemyReferences.playerLocationReference;
        agentLocation = transform;
    }

    void Update()
    {
        //check whether player is in sight and whether it's within attack range
        playerInSightRange = Vector3.Distance(agentLocation.position, playerLocation.position) <= sightRange;
        playerInAttackRange = Vector3.Distance(agentLocation.position, playerLocation.position) <= attackRange;

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = agentLocation.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        float randomY = Random.Range(-walkPointRange, walkPointRange);
        float randomZ = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(agentLocation.position.x + randomX, agentLocation.position.y + randomY, agentLocation.position.z + randomZ);

        //Ensure destination is valid
        if (NavMesh.SamplePosition(walkPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        UpdatePath();
    }

    private void AttackPlayer()
    {
        //Make sure enemy does not move
        agent.SetDestination(agentLocation.position);
        agentLocation.LookAt(playerLocation);
    }

    private void UpdatePath()
    {
        if (Time.time >= pathUpdateDeadline)
        {
            Debug.Log("Updating Path");
            pathUpdateDeadline = Time.time + pathUpdateDelay;
            agent.SetDestination(playerLocation.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (transform == null) return;

        //Show the Attack range and the Sight range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    public void Kill() // Function to kill the enemy
    {
        Instantiate(DeadEnemy, transform.position, transform.rotation); // Spawn in the dead enemy
        Destroy(gameObject); // Destroy the object to stop it getting in the way
    }
}
