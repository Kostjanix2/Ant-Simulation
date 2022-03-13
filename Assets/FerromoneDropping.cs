using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FerromoneDropping : MonoBehaviour
{
    [SerializeField, Range(0, 20f)]
    private float dropFerromoneTimeSteps = 3f;

    [SerializeField]
    private Transform foodFerromonePrefab;

    [SerializeField]
    private Transform homeFerromonePrefab;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DropFerromones());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DropFerromones()
    {
        while (true)
        {
            LayerMask mask = LayerMask.GetMask("deafult");
            Transform prefab = null;

            switch (GetComponent<AntBehaviour>().CurrentState)
            {
                case AntState.SearchingFood:
                    prefab = homeFerromonePrefab;
                    break;

                case AntState.SearchingHome:
                    prefab = foodFerromonePrefab;
                    break;
            }

            Transform ferromone = Instantiate(prefab, transform.position, new Quaternion());

            yield return new WaitForSeconds(dropFerromoneTimeSteps);
        }
    }
}
