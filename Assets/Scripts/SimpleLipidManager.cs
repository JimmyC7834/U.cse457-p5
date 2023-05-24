using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

public class SimpleLipidManager : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    [SerializeField] private Rigidbody _prefab;
    [SerializeField] private float _waitTime;
    
    [SerializeField] private Vector2Int _membraneSize;
    [SerializeField] private float _spacing;
    [SerializeField] private bool _showDebug;
    
    private Transform _transform;
    private ObjectPool<Rigidbody> _pool;
    private bool _initialized = false;
    
    private Rigidbody[] _lipids;
    private HashSet<Pair> _bonds;
    
    public int TotalLipidCount => _membraneSize.x * _membraneSize.y;
    
    private void Awake()
    {
        _transform = transform;
        _lipids = new Rigidbody[TotalLipidCount];
        _bonds = new HashSet<Pair>();
        _pool = new ObjectPool<Rigidbody>(
            () => Instantiate(_prefab, _transform) as Rigidbody,
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
                
                // add bonds
                if (j > 0) _bonds.Add(new Pair(index, index - 1));
                if (i > 0) _bonds.Add(new Pair(index, index - _membraneSize.y));
                if (j < _membraneSize.y - 1) _bonds.Add(new Pair(index, index + 1));
                if (i < _membraneSize.x - 1) _bonds.Add(new Pair(index, index + _membraneSize.y));
                
                if (i > 0 && j < _membraneSize.y - 1) 
                    _bonds.Add(new Pair(index, index - _membraneSize.y + 1));
                if (i < _membraneSize.x - 1 && j < _membraneSize.y - 1) 
                    _bonds.Add(new Pair(index, index + _membraneSize.y + 1));
            }
        }
        
        Debug.Log($"bond count: {_bonds.Count}, lipid count: {TotalLipidCount}" +
                  $", bond expected: {(_membraneSize.x - 1) * (_membraneSize.y - 1) * 4 + _membraneSize.x + _membraneSize.y - 2}");
        Assert.IsTrue(
            _bonds.Count == (_membraneSize.x - 1) * (_membraneSize.y - 1) * 4 + _membraneSize.x + _membraneSize.y - 2);

        Invoke(nameof(StartSimulation), _waitTime);
    }

    public void StartSimulation()
    {
        _initialized = true;
    }

    private void FixedUpdate()
    {
        if (!_initialized) return;

        PhysicsUpdate_SpringBonds();
    }

    public void PhysicsUpdate_SpringBonds()
    {
        // physics update
        foreach (Pair bond in _bonds)
            BondPhysicsUpdate(bond.Fst, bond.Snd);
        
        // simulate drag
        foreach (Rigidbody lipid in _lipids)
            lipid.velocity *= _profile.VelocityRestitution;
    }

    public void BondPhysicsUpdate(int fst, int snd)
    {
        float k = _profile.SpringConstant;
        float lipidR = _profile.LipidRadius;
        float eqmDist = _profile.EqmDist;

        Rigidbody self = _lipids[fst];
        Rigidbody other = _lipids[snd];

        float dist = Vector3.Distance(other.position, self.position);
        
        // total contraction force of the spring 
        float forceMag = k * (dist - lipidR * 2 - eqmDist);

        Vector3 dir = (other.position - self.position).normalized;
        Vector3 v = forceMag * dir;
        
        // apply half of the total force to each end
        self.velocity += v / 2 - _profile.Damping * self.velocity;
        other.velocity += -v / 2 - _profile.Damping * other.velocity;

        if (_showDebug) Debug.DrawLine(self.position, other.position);
    }
}
