using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseDisplay : MonoBehaviour
{
    public float offset;
    List<Vector3> positions = new List<Vector3>();
    Transform glowTransform;
    int currentPhase = -1;

    // Start is called before the first frame update
    void Start()
    {
        var phases = transform.Find("Phases").GetComponentsInChildren<BoxCollider>();
        var count = phases.Length;
        var cl = cardLayout(count);
        for (int i = 0; i < count; ++i) {
            var width = phases[i].GetComponent<BoxCollider>().bounds.size.x;
            var worldPos = new Vector3(width * cl[i], 0, 0) + transform.position;
            positions.Add(worldPos);
            phases[i].transform.position = worldPos;
        }
        glowTransform = transform.Find("Glow").transform;
        GS.ga_global.phaseActionHandler.before.on(PhaseActionKey.ENTER, (_) => {
            currentPhase += 1;
            if (currentPhase == count) currentPhase = 0;
            glowTransform.position = positions[currentPhase];
        });
    }

    float[] cardLayout(int count) {
        // 0: []
        // 1: [0]
        // 2: [-0.5, 0.5]
        // 3: [-1, 0, 1]
        // 4: [-1.5, -0.5, 0.5, 1.5]
        float[] res = new float[count];
        for(var i = 0; i < count; ++i) {
            res[i] = (-((count - 1) / 2.0f) + i) * (1 + offset); 
        }
        return res;
    }
}
