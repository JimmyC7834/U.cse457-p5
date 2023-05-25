﻿using UnityEngine;

public class MembraneSimulation : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    [SerializeField] private GameObject _prefab;

    [SerializeField] private bool _showDebug;
    [SerializeField] private Vector2Int _membraneSize;
    [SerializeField] private float _spacing;
    private PhysicsObject[] _lipids;
    private SoftBodySimulation _softBodySimulation;

    public int TotalLipidCount => _membraneSize.x * _membraneSize.y;

    private void Awake()
    {
        _softBodySimulation = new SoftBodySimulation();
        _lipids = new PhysicsObject[TotalLipidCount];
    }

    private void Start()
    {
        InitMembrane();
        _softBodySimulation.Enabled = true;
        _softBodySimulation.ShowDebug = _showDebug;
    }

    private void FixedUpdate()
    {
        _softBodySimulation.Update();
    }

    private SpringJoint CreateJoint(PhysicsObject fst, PhysicsObject snd) =>
        new SpringJoint(fst, snd, _profile.SpringConstant, _profile.RestLength,
            _profile.MaxLength, _profile.MinLength, _profile.Damp, true);
    
    private PhysicsObject CreateLipid(Vector3 position) =>
        new PhysicsObject(_profile.LipidMass, 0f, position, Vector3.zero, Instantiate(_prefab,transform));

    private void InitMembrane()
    {
        // add nodes
        Vector3 offset = new Vector3(-_spacing * _membraneSize.x / 2, 0, -_spacing * _membraneSize.y / 2);
        for (int i = 0; i < _membraneSize.x; i++)
        {
            for (int j = 0; j < _membraneSize.y; j++)
            {
                int index = i * _membraneSize.y + j;
                Vector3 pos = new Vector3(
                    i * _spacing,
                    transform.position.y,
                    _spacing * j) + offset;

                PhysicsObject node = CreateLipid(pos);
                _softBodySimulation.AddNode(node);
                _lipids[index] = node;
            }
        }
        
        // add bonds
        for (int i = 0; i < _membraneSize.x; i++)
        {
            for (int j = 0; j < _membraneSize.y; j++)
            {
                int index = i * _membraneSize.y + j;
                
                // add bonds
                if (j < _membraneSize.y - 1)
                    _softBodySimulation.AddJoint(CreateJoint(
                            _lipids[index],
                            _lipids[index + 1]
                        ));
                
                if (i < _membraneSize.x - 1)
                    _softBodySimulation.AddJoint(CreateJoint(
                        _lipids[index],
                        _lipids[index + _membraneSize.y]
                    ));
                
                if (j < _membraneSize.y - 1 && i < _membraneSize.x - 1)
                    _softBodySimulation.AddJoint(CreateJoint(
                        _lipids[index],
                        _lipids[index + _membraneSize.y + 1]
                    ));
                
                if (j > 0 && i < _membraneSize.x - 1)
                    _softBodySimulation.AddJoint(CreateJoint(
                        _lipids[index],
                        _lipids[index + _membraneSize.y - 1]
                    ));
            }
        }
    }
}