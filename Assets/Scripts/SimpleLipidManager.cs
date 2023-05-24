using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class SimpleLipidManager : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;

    [SerializeField] private Vector2Int _membraneSize;
    [SerializeField] private float _spacing;
    [SerializeField] private int _neighborCount;
    [SerializeField] private SimpleLipid _prefab;
    private Transform _transform;
    private ObjectPool<SimpleLipid> _pool;
    private SimpleLipid[] _lipids;
    private bool _initialized = false;

    public int TotalLipidCount => _membraneSize.x * _membraneSize.y;
    
    private void Awake()
    {
        _transform = transform;
        _lipids = new SimpleLipid[TotalLipidCount];
        _pool = new ObjectPool<SimpleLipid>(
            () => Instantiate(_prefab, _transform) as SimpleLipid,
            (x) => x.gameObject.SetActive(true),
            (x) => x.gameObject.SetActive(false));
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        for (int i = 0; i < _membraneSize.x; i++)
        {
            for (int j = 0; j < _membraneSize.y; j++)
            {
                int index = i * _membraneSize.y + j;
                _lipids[index] = _pool.Get();
                _lipids[index].transform.position = new Vector3(
                    i * _spacing - ((j % 2 == 1) ? _spacing / 2 : 0),
                    _transform.position.y,
                    _spacing * j);
                
                if (j > 0) _lipids[index].NeighborIndices.Add(index - 1);
                if (i > 0) _lipids[index].NeighborIndices.Add(index - _membraneSize.y);
                if (j < _membraneSize.y - 1) _lipids[index].NeighborIndices.Add(index + 1);
                if (i < _membraneSize.x - 1) _lipids[index].NeighborIndices.Add(index + _membraneSize.y);
                
                if (i > 0 && j < _membraneSize.y - 1)
                    _lipids[index].NeighborIndices.Add(index - _membraneSize.y + 1);
                
                if (i < _membraneSize.x - 1 && j < _membraneSize.y - 1)
                    _lipids[index].NeighborIndices.Add(index + _membraneSize.y + 1);
            }
        }

        Invoke(nameof(StartSimulation), 10f);
    }

    public void StartSimulation()
    {
        _initialized = true;
    }

    private void FixedUpdate()
    {
        if (!_initialized) return;
        
        foreach (SimpleLipid lipid in _lipids)
        {
            PhysicsUpdate_SpringNet(lipid);
        }
    }

    public void PhysicsUpdate_Spring(SimpleLipid lipid)
    {
        float attractionRadius = _profile.AttractionRadius;
        float k = _profile.AttractionForce;
        
        Collider[] results = Physics.OverlapSphere(lipid.Position, attractionRadius, LayerMask.GetMask("Lipid"));
        
        Vector3 totalForce = Vector3.zero;

        for (int i = 0; i < results.Length; i++)
        {
            Collider c = results[i];
            SimpleLipid other;
            if (!c.TryGetComponent<SimpleLipid>(out other)) continue;
            if (other == lipid) continue;

            float dist = Vector3.Distance(other.Position, lipid.Position);
            float forceMag = k * (dist - .3f);

            totalForce += forceMag * (other.Position - lipid.Position).normalized;
        }

        lipid.Rigidbody.velocity += totalForce;
        lipid.Rigidbody.velocity *= _profile.VelocityRestitution;
    }
    
    public void PhysicsUpdate_SpringNet(SimpleLipid lipid)
    {
        float attractionRadius = _profile.AttractionRadius;
        float k = _profile.AttractionForce;
        
        Vector3 totalForce = Vector3.zero;

        for (int i = 0; i < lipid.NeighborIndices.Count; i++)
        {
            SimpleLipid other = _lipids[lipid.NeighborIndices[i]];
            Debug.DrawLine(lipid.Position, other.Position);
            float dist = Vector3.Distance(other.Position, lipid.Position);
            float forceMag = k * (dist - _profile.LipidRadius - _profile.LipidRadius - _profile.EqmDist);
            totalForce += forceMag * (other.Position - lipid.Position).normalized;
        }

        totalForce.x *= _profile.ForceRestitution.x;
        totalForce.y *= _profile.ForceRestitution.y;
        totalForce.z *= _profile.ForceRestitution.z;
        lipid.Rigidbody.velocity += totalForce;
        lipid.Rigidbody.velocity *= _profile.VelocityRestitution;
    }
}
