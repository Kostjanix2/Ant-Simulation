using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomePocket : MonoBehaviour
{
    public int foodAmount = 0;
    public void addFood()
    {
        foodAmount++;
        GetComponent<ParticleSystem>().emissionRate = foodAmount;
    }
}
