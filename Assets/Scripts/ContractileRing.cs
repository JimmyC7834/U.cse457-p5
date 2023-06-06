using UnityEngine;

public class ContractileRing : MonoBehaviour
{
    [SerializeField] private GameObject _sphere;
    [SerializeField] private LineRenderer _LineRenderer;
    [SerializeField] private float _height = 5f;

    [SerializeField] private GameObject[] _cylinders;
    private Vector3[] originalPositions;

    // Start is called before the first frame update
    void Start()
    {
        float sphereInitialY = _sphere.transform.position.y;
        float ringInitialY = gameObject.transform.position.y;
        // _height = sphereInitialY - ringInitialY;

        originalPositions = new Vector3[_cylinders.Length];

        // Store the references to the child cylinders and their original positions
        for (int i = 0; i < _cylinders.Length; i++)
        {
            originalPositions[i] = _cylinders[i].transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_sphere) return;
        
        float sphereY = _sphere.transform.position.y;
        float heightChange = _height - sphereY - 5.5f;
        
        Vector3[] positions = new Vector3[_cylinders.Length];

        // Update the positions of the cylinders based on the original positions
        for (int i = 0; i < _cylinders.Length; i++)
        {
            Vector3 direction = gameObject.transform.position - originalPositions[i];
            direction = direction.normalized;

            Vector3 newPos = originalPositions[i] + direction * heightChange;
            _cylinders[i].transform.position = newPos;
            positions[i] = _cylinders[i].transform.position;
        }
        
        _LineRenderer.SetPositions(positions);

    }
}