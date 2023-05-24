using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LipidManager : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;

    [SerializeField] private Vector2Int _membraneSize;
    [SerializeField] private float _spacing;
    [SerializeField] private LipidController _prefab;
    private Transform _transform;
    private ObjectPool<LipidController> _pool;
    private LipidController[] _lipids;
    private bool _initialized = false;

    public int TotalLipidCount => _membraneSize.x * _membraneSize.y;
    
    private void Awake()
    {
        _transform = transform;
        _lipids = new LipidController[TotalLipidCount];
        
        _pool = new ObjectPool<LipidController>(
            () => Instantiate(_prefab, _transform) as LipidController,
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
                int index = i * _membraneSize.x + j;
                _lipids[index] = _pool.Get();
                _lipids[index].transform.position = new Vector3(
                    i * _spacing,
                    _transform.position.y,
                    _spacing * j);
            }
        }

        _initialized = true;
    }

    private void FixedUpdate()
    {
        if (!_initialized) return;
        
        foreach (LipidController lipid in _lipids)
        {
            PhysicsUpdate(lipid);
        }
    }
    
    public void PhysicsUpdate(LipidController lipid)
    {
        float attractionRadius = _profile.AttractionRadius;

        Collider[] results = Physics.OverlapSphere(lipid.Position, attractionRadius);
        Vector3 totalHeadV = Vector3.zero;
        Vector3 totalTailV = Vector3.zero;

        foreach (Collider c in results)
        {
            LipidController other;
            if (!c.TryGetComponent<LipidController>(out other)) continue;
            if (other == lipid) continue;

            // float headF = 
            //     _profile.HeadAttractionForce(Vector3.Distance(other.HeadPosition, lipid.HeadPosition));
            //
            // float tailF =
            //     _profile.TailAttractionForce(Vector3.Distance(other.TailPosition, lipid.TailPosition));
            
            float headF = 1f / Vector3.Distance(other.HeadPosition, lipid.HeadPosition);
            
            float tailF = 1f / Vector3.Distance(other.TailPosition, lipid.TailPosition);

            
            totalHeadV += headF * (other.HeadPosition - lipid.Position).normalized;
            totalTailV += tailF * (other.TailPosition - lipid.Position).normalized;
        }

        lipid.HeadRigidbody.velocity = totalHeadV;
        lipid.TailRigidbody.velocity = totalTailV;
    }
    
    public void PhysicsUpdate2(LipidController lipid)
    {
        float attractionRadius = _profile.AttractionRadius;
        float attractionForce = _profile.AttractionForce;

        Vector3 totalHeadV = Vector3.zero;
        Vector3 totalTailV = Vector3.zero;

        foreach (LipidController other in _lipids)
        {
            if (other == lipid) continue;
            float headDist = Vector3.Distance(other.HeadPosition, lipid.HeadPosition);
            float tailDist = Vector3.Distance(other.TailPosition, lipid.TailPosition);
            
            if ((headDist + tailDist) / 2 < attractionRadius) continue;

            float headF = attractionForce * 1f / (headDist * headDist);
            float tailF = attractionForce * 1f / (tailDist * tailDist);
            
            totalHeadV += headF * (other.HeadPosition - lipid.Position).normalized;
            totalTailV += tailF * (other.TailPosition - lipid.Position).normalized;
        }

        lipid.HeadRigidbody.velocity = totalHeadV;
        lipid.TailRigidbody.velocity = totalTailV;
    }
}
