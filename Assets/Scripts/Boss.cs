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

    private void updateState()
    {
        var newStateRandom = Random.Range(0, 100);
        Debug.Log(newStateRandom);
        if (newStateRandom < stompingProbability) {
            currentState = State.Stomping;
            Ub.SetBool("Stomping", true);
            Debug.Log("Stomping");
        } else if (newStateRandom < stompingProbability + idleProbability) {
            currentState = State.Idle;
            Debug.Log("Idle");
        } else if (newStateRandom >= stompingProbability + idleProbability) {
            Ub.SetBool("Roaring", true);
            currentState = State.Roaring;
            Debug.Log("Roaring");
        }
    }
}
