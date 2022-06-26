using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeObject : MonoBehaviour
{
    AbstractCardGameController controller;
    TMPro.TextMeshPro lifeGui;
    int lifeTotal;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponentInParent<AbstractCardGameController>();
        lifeGui = GetComponent<TMPro.TextMeshPro>();
        // Override rotation to "face" the camera
        // there's probably a better way
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, 0, transform.eulerAngles.z);
    }

    // Update is called once per frame
    void Update()
    {
        lifeTotal = controller.player.lifepoints;
        lifeGui.text = lifeTotal.ToString();
    }
}
