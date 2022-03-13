using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ferromone : MonoBehaviour
{
    [SerializeField, Range(0, 40f)]
    private float time = 10f;

    SpriteRenderer rendererS;

    // Start is called before the first frame update
    void Awake()
    {
        rendererS = GetComponent<SpriteRenderer>();
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        float timeStep = 0.1f;
        float dxa = 1 / (time / timeStep);

        Color c = rendererS.color;
        for (float alpha = 1f; alpha >= 0; alpha -= dxa)
        {
            c.a = alpha;
            rendererS.color = c;
            yield return new WaitForSeconds(timeStep);
        }

        Destroy(this.gameObject);
    }
}
