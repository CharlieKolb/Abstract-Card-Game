using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class TargetSelecter : MonoBehaviour
{
    public EffectTarget effectTarget;

    public GameObject selected;

    public void Init(EffectTarget target) {
        effectTarget = target;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
