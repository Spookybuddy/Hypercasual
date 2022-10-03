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
    private bool makeMesh;

    //Manager
    public GameObject manager;
    private GameManager script;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        collision = GetComponent<MeshCollider>();
        script = manager.GetComponent<GameManager>();

        drawn.positionCount = maxLength;
        drawn.startWidth = 0.1f;
        drawn.endWidth = 0.25f;

        sumDistance = 0;
    }

    void Update()
    {
        mouseDown = Input.GetMouseButton(0);
        view = main.ScreenPointToRay(Input.mousePosition);

        if (script.rainbowMode) Rainbow();
        else Normal();

        //Get tap position and add it to a list
        if (mouseDown && Physics.Raycast(view, out RaycastHit hit) && (script.canDraw || script.canDoodle)) {
            if (Vector3.Distance(add, new Vector3(hit.point.x, hit.point.y, 0)) > 0.001f) {
                add = new Vector3(hit.point.x, hit.point.y, 0);
                points.Add(add);
                makeMesh = true;
            }
        }

        //Add the distances between all points
        sumDistance = 0;
        for (int p = 1; p < points.Count; p++) sumDistance += Vector3.Distance(points[p - 1], points[p]);

        //Distance of line will shrink after no longer tapping
        if (newDown && sumDistance > maxDistance) {
            points.RemoveAt(0);
            if (points.Count < maxLength) points.Add(add);
        }

        //Get list length and remove first points when length is too big
        if (points.Count > maxLength) points.RemoveAt(0);

        //When stop tapping, next tap will be a new line
        if (!mouseDown) newDown = true;

        //Line fades when doodling on Main
        if (newDown && script.canDoodle) {
            points.RemoveAt(0);
            makeMesh = false;
            if (points.Count < maxLength) points.Add(add);
        }

        //When new tap, set every point to the tap location
        if (mouseDown && newDown) {
            newDown = false;
            makeMesh = false;
            for (int i = 0; i < maxLength; i++) points[i] = add;
        }

        //Convert list to array and set line positions
        coords = points.ToArray();
        drawn.SetPositions(coords);

        //Bake Mesh collider to interact with physics
        mesh.Clear();
        filters.mesh = empty;
        collision.sharedMesh = empty;
        if (makeMesh) {
            drawn.BakeMesh(mesh, main, true);
            filters.mesh = mesh;
            collision.sharedMesh = mesh;
        }
    }

    private void Normal()
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] normals = new GradientColorKey[1];
        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        normals[0] = new GradientColorKey(Color.white, 1);
        alphas[0] = new GradientAlphaKey(1, 0);
        alphas[1] = new GradientAlphaKey(1, 1);
        drawn.colorGradient = gradient;
    }

    private void Rainbow()
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] rainbows = new GradientColorKey[7];
        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        rainbows[0] = new GradientColorKey(Color.red, 0);
        rainbows[1] = new GradientColorKey(Color.yellow, 0.166f);
        rainbows[2] = new GradientColorKey(Color.green, 0.333f);
        rainbows[3] = new GradientColorKey(Color.cyan, 0.5f);
        rainbows[4] = new GradientColorKey(Color.blue, 0.666f);
        rainbows[5] = new GradientColorKey(Color.magenta, 0.833f);
        rainbows[6] = new GradientColorKey(Color.red, 1);
        alphas[0] = new GradientAlphaKey(1, 0);
        alphas[1] = new GradientAlphaKey(1, 1);
        gradient.SetKeys(rainbows, alphas);
        drawn.colorGradient = gradient;
    }
}