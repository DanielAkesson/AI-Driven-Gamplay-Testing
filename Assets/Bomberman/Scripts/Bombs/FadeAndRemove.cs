using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeAndRemove : MonoBehaviour
{
    public float FadeTime;
    public Renderer Renderer;
    private float CurrentTime;
    private Color StartColor;
    void Start()
    {
        StartColor = Renderer.material.GetColor("_Color");
    }

    void FixedUpdate()
    {
        CurrentTime += Arena.TimeStep;

        if (CurrentTime >= FadeTime)
        {
            Destroy(this.gameObject);
            return;
        }
        
        float factor = 1f-(CurrentTime / FadeTime);
        Color newC = StartColor;
        newC.a *= factor;
        Renderer.material.SetColor("_Color", newC);
    }
}
