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
    private float offset;

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
        offset = 0;
    }

    void Update()
    {
        mouseDown = Input.GetMouseButton(0);
        view = main.ScreenPointToRay(Input.mousePosition);

        switch (script.mode) {
            case 0:
                White();
                break;
            case 1:
                Grey();
                break;
            case 2:
                Black();
                break;
            case 3:
                Graydient();
                break;
            case 4:
                Greydient();
                break;
            case 5:
                Red();
                break;
            case 6:
                Orange();
                break;
            case 7:
                Yellow();
                break;
            case 8:
                Green();
                break;
            case 9:
                DarkGreen();
                break;
            case 10:
                Cyan();
                break;
            case 11:
                Blue();
                break;
            case 12:
                Magenta();
                break;
            case 13:
                Purple();
                break;
            case 14:
                Rainbow();
                break;
            case 15:
                offset += 10 * Time.deltaTime;
                Disco(Mathf.RoundToInt(offset));
                break;
            default:
                White();
                break;
        }

        //Get tap position and add it to a list
        if (mouseDown && Physics.Raycast(view, out RaycastHit hit) && (script.canDraw || script.canDoodle)) {
            if (Vector3.Distance(add, new Vector3(hit.point.x, hit.point.y, 0)) > 0.001f) {
                if (!newDown) script.Scroll(hit.point.y - add.y);
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

    //Solid grayscale colors
    private void White() { SetGradient(Solid(Color.white)); }

    private void Grey() { SetGradient(Solid(Color.gray)); }

    private void Black() { SetGradient(Solid(Color.black)); }

    private void Graydient() {
        Color[] list = new Color[3] { Color.white, Color.gray, Color.black };
        SetGradient(Grad(list, 3));
    }

    private void Greydient()
    {
        Color[] list = new Color[3] { Color.black, Color.gray, Color.white };
        SetGradient(Grad(list, 3));
    }

    //Solid Basic colors
    private void Red() { SetGradient(Solid(Color.red)); }

    private void Orange() { SetGradient(Solid(new Color(1, 0.502f, 0))); }

    private void Yellow() { SetGradient(Solid(Color.yellow)); }

    private void Green() { SetGradient(Solid(Color.green)); }

    private void DarkGreen() { SetGradient(Solid(new Color(0.106f, 0.553f, 0.239f))); }

    private void Cyan() { SetGradient(Solid(Color.cyan)); }

    private void Blue() { SetGradient(Solid(Color.blue)); }

    private void Magenta() { SetGradient(Solid(Color.magenta)); }

    private void Purple() { SetGradient(Solid(new Color(0.639f, 0.286f, 0.643f))); }

    private void Rainbow() {
        Color[] list = new Color[7] { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta, Color.red };
        SetGradient(Grad(list, 7));
    }

    private void Disco(int count) {
        Color[] list = new Color[7] { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta, Color.red };
        GradientColorKey[] colour = new GradientColorKey[7];
        for (int i = 0; i < 7; i++) colour[i] = new GradientColorKey(list[(i + count) % 7], i / 6f);
        SetGradient(colour);
    }

    //Return a solid color
    private GradientColorKey[] Solid(Color c) {
        return new GradientColorKey[1] { new GradientColorKey(c, 1) };
    }

    //Return a gradient color
    private GradientColorKey[] Grad(Color[] c, int length) {
        GradientColorKey[] colour = new GradientColorKey[length];
        for (int i = 0; i < length; i++) colour[i] = new GradientColorKey(c[i], i / (float)(length - 1));
        return colour;
    }

    //Return the alpha
    private GradientAlphaKey[] AllAlphas() {
        return new GradientAlphaKey[2] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) };
    }

    //Set the desired values
    private void SetGradient(GradientColorKey[] C) {
        Gradient gradient = new Gradient();
        gradient.SetKeys(C, AllAlphas());
        drawn.colorGradient = gradient;
    }
}