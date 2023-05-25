using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

public class SimpleLipidManager : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    [SerializeField] private Rigidbody _prefab;
    [SerializeField] private float _waitTime;

    [SerializeField] private Rigidbody _food;
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
        _lipids = new Rigidbody[TotalLipidCount + 1];
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
        Vector3 offset = new Vector3(-_spacing * _membraneSize.x / 2, 0, -_spacing * _membraneSize.y / 2);
        for (int i = 0; i < _membraneSize.x; i++)
        {
            for (int j = 0; j < _membraneSize.y; j++)
            {
                int index = i * _membraneSize.y + j;
                _lipids[index] = _pool.Get();
                _lipids[index].transform.position = new Vector3(
                    i * _spacing - ((j % 2 == 1) ? _spacing / 2 : 0),
                    _transform.position.y,
                    _spacing * j) + offset;
                
                // add bonds
                if (j > 0) _bonds.Add(new Pair(index, index - 1));
                if (i > 0) _bonds.Add(new Pair(index, index - _membraneSize.y));
                if (j < _membraneSize.y - 1) _bonds.Add(new Pair(index, index + 1));
                if (i < _membraneSize.x - 1) _bonds.Add(new Pair(index, index + _membraneSize.y));
                
                // if (i > 0 && j < _membraneSize.y - 1) 
                //     _bonds.Add(new Pair(index, index - _membraneSize.y + 1));
                // if (i < _membraneSize.x - 1 && j < _membraneSize.y - 1) 
                //     _bonds.Add(new Pair(index, index + _membraneSize.y + 1));
            }
        }

        _lipids[TotalLipidCount] = _food;
        
        // Debug.Log($"bond count: {_bonds.Count}, lipid count: {TotalLipidCount}" +
        //           $", bond expected: {(_membraneSize.x - 1) * (_membraneSize.y - 1) * 4 + _membraneSize.x + _membraneSize.y - 2}");
        // Assert.IsTrue(
        //     _bonds.Count == (_membraneSize.x - 1) * (_membraneSize.y - 1) * 4 + _membraneSize.x + _membraneSize.y - 2);

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
            BondPhysicsUpdate(_lipids[bond.Fst], _lipids[bond.Snd]);
        
        // simulate drag
        foreach (Rigidbody lipid in _lipids)
            lipid.velocity *= _profile.VelocityRestitution;
    }

    public void BondPhysicsUpdate(Rigidbody self, Rigidbody other)
    {
        float maxDist = _profile.MaxLength;
        
        float k = _profile.SpringConstant;
        float eqmDist = _profile.RestLength;
        
        float dist = Vector3.Distance(other.position, self.position);
        Vector3 dir = (other.position - self.position).normalized;
        
        if (dist > maxDist)
        {
            float dD = dist - maxDist;
            Vector3 dx = dD * -.5f * dir;
            self.position += -dx;
            other.position += dx;
        }

        // total contraction force of the spring 
        float forceMag = k * (dist - eqmDist);

        Vector3 v = forceMag * dir;

        // if (self.position.y < -.75f) v += -(self.position - Vector3.down).normalized * 3f;
        // if (other.position.y < -.75f) v += -(other.position - Vector3.down).normalized * 3f;
        
        // apply half of the total force to each end
        self.velocity += v / 2 - _profile.Damp * self.velocity;
        other.velocity += -v / 2 - _profile.Damp * other.velocity;
        
        if (_showDebug) Debug.DrawLine(self.position, other.position);
    }
}