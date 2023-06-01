using UnityEngine;

public class ContractileRing : MonoBehaviour
{
    public GameObject sphere;
    public GameObject ring;
    public GameObject cylinderPrefab;
    public int numCylinders = 16;
    public float height = 5f;
    public float distance = 5f;

    private GameObject[] cylinders;
    private Vector3[] originalPositions;

    // Start is called before the first frame update
    void Start()
    {
        sphere = GameObject.Find("Sphere");
        ring = gameObject;

        float sphereInitialY = sphere.transform.position.y;
        float ringInitialY = ring.transform.position.y;
        height = sphereInitialY - ringInitialY;

        cylinders = new GameObject[numCylinders];
        originalPositions = new Vector3[numCylinders];

        // Store the references to the child cylinders and their original positions
        for (int i = 0; i < ring.transform.childCount; i++)
        {
            cylinders[i] = ring.transform.GetChild(i).gameObject;
            originalPositions[i] = cylinders[i].transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!sphere) return;
        
        float sphereY = sphere.transform.position.y;
        float heightChange = height - sphereY - 5.5f;

        // Update the positions of the cylinders based on the original positions
        for (int i = 0; i < numCylinders; i++)
        {
            Vector3 direction = ring.transform.position - originalPositions[i];
            direction = direction.normalized;

            Vector3 newPos = originalPositions[i] + direction * heightChange;
            cylinders[i].transform.position = newPos;
        }
    }
}