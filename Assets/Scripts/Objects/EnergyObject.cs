using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyObject : MonoBehaviour
{
    AbstractCardGameController controller;
    TMPro.TextMeshPro energyGui;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponentInParent<AbstractCardGameController>();
        energyGui = GetComponent<TMPro.TextMeshPro>();
        // Override rotation to "face" the camera
        // there's probably a better way
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, 0, transform.eulerAngles.z);
    }

    string getText(string resourceName, int current, int max) {
        if (max == 0) {
            return "";
        }
        return "<color=" + resourceName + ">" + current + "/" + max + "</color>\n";
    }

    // Update is called once per frame
    void Update()
    {
        string newText = "";
        var energy = controller.player.side.energy;
        var maxEnergy = controller.player.side.maxEnergy;

        newText += getText("red", energy.red, maxEnergy.red); 
        newText += getText("green", energy.green, maxEnergy.green); 
        newText += getText("blue", energy.blue, maxEnergy.blue); 
        energyGui.text = newText;
    }
}
