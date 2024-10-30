using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleWood : MonoBehaviour
{
    [SerializeField] private int woodPoints = 5;

    public int GetWoodPoints()
    {
        return woodPoints;
    }
}