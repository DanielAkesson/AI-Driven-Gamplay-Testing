using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{   [HideInInspector]
    public Arena Arena;
    public GameObject PickUp;

    public void Hit(BombController bomb)
    {
        Arena.DestroyGameObject(this.gameObject);
        GameObject obj = Instantiate(PickUp, transform.position, Quaternion.identity, Arena.transform);
        Arena.RegisterGameObject(obj);
    }
}
