using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombDummy : MonoBehaviour
{
    public Renderer BombRender;
    public float FuseTime;
    public void OcsolateColor(float CurrentFuseTime)
    {
        float sin = 1;
        float factor = CurrentFuseTime / FuseTime;
        if (CurrentFuseTime > .2f)
            sin = Mathf.Sin(8f / CurrentFuseTime);

        //Change color
        Color color = Color.red * Mathf.Clamp01(sin * sin - factor);
        BombRender.material.SetColor("_EmissionColor", color);
    }
}
