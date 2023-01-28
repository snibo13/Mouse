using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    private enum State {
        Stomping,
        Roaring,
        Idle
    }


    public int stompingProbability = 30;
    public int idleProbability = 50;
    public float stateChangeTime = 0.5f;
    // Time between state changes when idle
    private State initialState = State.Idle;
    private State currentState;
    private Animator Ub;
    private float lastUpdateTime;
    public float delay;
    public float spawnOffset;
    private int face = -1;
    public GameObject wavePrefab;
    public GameObject roarPrefab;
    private GameObject wave;
    // Start is called before the first frame update
    void Start()
    {
        Random.seed = 213;
        currentState = initialState;
        lastUpdateTime = Time.time + delay; // Initial delay before acting
        Ub = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        clearAnimatorVariables();
        if (Time.time - lastUpdateTime >= stateChangeTime) {
            
            updateState();
            Debug.Log("Update State");
            lastUpdateTime = Time.time;
        }
    }

    private void clearAnimatorVariables()
    {
        Ub.SetBool("Roaring", false);
        Ub.SetBool("Stomping", false);
    }
    private bool spawned = false;
    public float spawnDelay = 4;

    private IEnumerator spawnFreeze()
    {
        yield return new WaitForSeconds(spawnDelay);
        spawned = false;
    }

    private void updateState()
    {
        var newStateRandom = Random.Range(0, 100);
        if (newStateRandom < stompingProbability) {
            currentState = State.Stomping;
            Ub.SetBool("Stomping", true);
            if (!spawned) {
                spawned = true;
                float distance = transform.position.x + face * spawnOffset;
                Vector2 spawnPoint = new Vector2(distance, 1.75f);
                
                GameObject newProjectile = (GameObject)Instantiate(
                    wavePrefab,
                    spawnPoint,
                    transform.rotation
                );
            }
            StartCoroutine("spawnFreeze");
            
            
        } else if (newStateRandom < stompingProbability + idleProbability) {
            currentState = State.Idle;
        } else if (newStateRandom >= stompingProbability + idleProbability) {
            Ub.SetBool("Roaring", true);
            currentState = State.Roaring;
            if (!spawned) {
                spawned = true;
                float distance = transform.position.x + face * spawnOffset;
                Vector2 spawnPoint = new Vector2(distance, 4f);
                
                GameObject newProjectile = (GameObject)Instantiate(
                    roarPrefab,
                    spawnPoint,
                    transform.rotation
                );
            }
            StartCoroutine("spawnFreeze");
            Debug.Log("Roar");
        }
    }
}
