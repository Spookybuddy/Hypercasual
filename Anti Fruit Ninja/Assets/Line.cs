using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    //Line drawing components
    public LineRenderer drawn;
    public Mesh mesh;
    public MeshFilter filters;
    public MeshCollider collision;
    public Mesh empty;
    public Camera main;
    private Ray view;

    //Coordinates
    public Vector3 add;
    public List<Vector3> points;
    private Vector3[] coords;

    //Variables
    public int maxLength;
    public int maxDistance;
    public float sumDistance;

    //New tap check
    private bool mouseDown;
    private bool newDown;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        collision = GetComponent<MeshCollider>();

        drawn.positionCount = maxLength;
        drawn.startWidth = 0.1f;
        drawn.endWidth = 0.25f;

        sumDistance = 0;
    }

    void Update()
    {
        mouseDown = Input.GetMouseButton(0);
        view = main.ScreenPointToRay(Input.mousePosition);

        //Get tap position and add it to a list
        if (mouseDown && Physics.Raycast(view, out RaycastHit hit)) {
            if (Vector3.Distance(add, new Vector3(hit.point.x, hit.point.y, 0)) > 0.001f) {
                add = new Vector3(hit.point.x, hit.point.y, 0);
                points.Add(add);
            }
        }

        //Add the distances between all points
        sumDistance = 0;
        for (int p = 1; p < points.Count; p++) {
            sumDistance += Vector3.Distance(points[p - 1], points[p]);
        }

        //Distance of line will shrink after no longer tapping
        if (newDown && sumDistance > maxDistance) {
            points.RemoveAt(0);
            if (points.Count < maxLength) {
                points.Add(add);
            }
        }

        //Get list length and remove first points when length is too big
        if (points.Count > maxLength) {
            points.RemoveAt(0);
        }

        //When stop tapping, next tap will be a new line
        if (!mouseDown) {
            newDown = true;
        }

        //When new tap, set every point to the tap location
        if (mouseDown && newDown) {
            newDown = false;
            for (int i = 0; i < maxLength; i++) {
                points[i] = add;
            }
        }

        //Convert list to array and set line positions
        coords = points.ToArray();
        drawn.SetPositions(coords);

        //Bake Mesh collider to interact with physics
        mesh.Clear();
        if (sumDistance > 0) {
            Triggering(false);
            drawn.BakeMesh(mesh, main, true);
            filters.mesh = mesh;
            collision.sharedMesh = mesh;
        } else {
            //Set collision to other placeholder
            Triggering(true);
            filters.mesh = empty;
            collision.sharedMesh = empty;
        }
    }

    private void Triggering(bool onOff)
    {
        collision.convex = onOff;
        collision.isTrigger = onOff;
    }
}