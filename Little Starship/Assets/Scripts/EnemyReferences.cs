using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyReferences : MonoBehaviour
{
    public GameObject playerReference { get; private set; }
    public Transform playerLocationReference { get; private set; }
    //public static EnemyReferences Instance { get; private set; }

    private void Awake()
    {
        //if (Instance == null)
        //{
        //    Instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //}

        playerReference = GameObject.FindGameObjectWithTag("Player");
        playerLocationReference = playerReference.transform;
    }

    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
