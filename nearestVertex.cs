using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

[RequireComponent(typeof(DangerPathFinder))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LineRenderer))]
public class nearestVertex : MonoBehaviour {

    
    public NavMeshAgent navMeshAgent;
    public LineRenderer lineRenderer;
    public Vector3 losPos;

    public int posCount;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update () {

        posCount = lineRenderer.positionCount;

        if (posCount <= 2)
        {
            // if there is direct LOS, move to parent transform
            losPos = GetComponentInParent<Transform>().position;
        }
        else
        {
            // positions is the number of vertices on the line
            losPos = lineRenderer.GetPosition(posCount - 2);
        }
	}
}
