using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LipidManager : MonoBehaviour
{
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
            lipid.PhysicsUpdate();
        }
    }
}
