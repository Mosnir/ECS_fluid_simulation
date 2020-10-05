using SPH.Spawners;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToyManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ToySystem.toy = this.gameObject;
    }

}
