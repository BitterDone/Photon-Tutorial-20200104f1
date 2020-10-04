using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointUpdater : MonoBehaviour
{

    public Transform pointBody;
    public Transform[] childJoints;
    // Start is called before the first frame update
    void Start()
    {
        int numChildren = pointBody.childCount;
        childJoints = new Transform[numChildren];

        for(int i = 0; i < numChildren; i++)
        {
            childJoints[i] = pointBody.GetChild(i);
        }
    }

    public void UpdateJointPos(Dictionary<int, Vector3> indexPair)
    {
        //Debug.Log("Updating Joint Pos");
        int dictCount = indexPair.Count;

        for(int i = 0; i < dictCount; i++)
        {
            childJoints[i].localPosition = indexPair[i];            
        }
    }
}
