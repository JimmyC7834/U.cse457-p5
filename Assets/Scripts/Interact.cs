using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] private GameObject _sphere;
    [SerializeField] private float _spawnHeight;
    
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Instantiate(
                    _sphere,
                    new Vector3(hit.point.x, _spawnHeight, hit.point.z),
                    Quaternion.identity
                );
            }
        }
    }
}
