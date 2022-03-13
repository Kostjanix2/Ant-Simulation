using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPocket : MonoBehaviour
{
    GameObject currentFood = null;

    public void addFood(GameObject food)
    {
        if(currentFood == null)
        {
            currentFood = Instantiate(food, this.transform);
            currentFood.SetActive(false);
            currentFood.transform.localPosition = new Vector3(0,0,0);
            Destroy(food);
        }
    }

    public void removeFood(GameObject home)
    {
        if (currentFood != null)
        {
            home.GetComponent<HomePocket>().addFood();
            Destroy(currentFood);
        }
    }
}
