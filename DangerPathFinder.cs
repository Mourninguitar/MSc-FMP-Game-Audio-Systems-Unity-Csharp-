using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(NavMeshAgent))]
public class DangerPathFinder : MonoBehaviour {

    public bool DebugMode;

    [HideInInspector] public LineRenderer lr;
    [HideInInspector] public NavMeshAgent NavAgent;

    public GameObject player;

    private float lengthSoFar;
    [HideInInspector] public float pathLength;
    [HideInInspector] public Vector3 losPos;
    [HideInInspector] public int posCount;
    private Vector3 playerCurrentPos;
    private Vector3 playerLastPos;
    private Vector3[] PathCorners;


    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        NavAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        playerCurrentPos = player.transform.position;

        //__________________________________________________________
        // Check if the player is moving before updating the path
        if (playerCurrentPos != playerLastPos)
        {
            NavAgent.SetDestination(playerCurrentPos);
        }
        //__________________________________________________________


        //__________________________________________________________
        // Draw a line showing the shortest path to the player
        if (NavAgent.hasPath)
        {
            lr.positionCount = NavAgent.path.corners.Length;
            lr.SetPositions(NavAgent.path.corners);

            Vector3 previousCorner = NavAgent.path.corners[0];
            lengthSoFar = 0f;
            int i = 1;
            while (i < NavAgent.path.corners.Length)
            {
                Vector3 currentCorner = NavAgent.path.corners[i];
                lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
                previousCorner = currentCorner;
                i++;

                pathLength = lengthSoFar;
            }
            lr.enabled = DebugMode;
        }
        //__________________________________________________________


        //__________________________________________________________
        posCount = lr.positionCount;

        if (posCount <= 2)
        {
            // if there is direct LOS, move to parent transform
            losPos = GetComponentInParent<Transform>().position;
        }
        else
        {
            // positions is the number of vertices on the line
            losPos = lr.GetPosition(posCount - 2);
        }
        //__________________________________________________________


        playerLastPos = player.transform.position;
    }
}
