using System.Collections.Generic;
using UnityEngine;

public class DemoSim : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    [SerializeField] private GameObject _prefab;

    [SerializeField] float _gravity;
    [SerializeField] private bool _showDebug;
    private List<PhysicsObject> _lipids;
    private SoftBodySimulation _softBodySimulation;
    
    private void Awake()
    {
        _softBodySimulation = new SoftBodySimulation();
        _lipids = new List<PhysicsObject>();
    }

    private void Start()
    {
        _lipids.Add(CreateLipid(new Vector3(0, 1, 0)));
        _lipids.Add(CreateLipid(new Vector3(1, 1, 0)));
        _lipids.Add(CreateLipid(new Vector3(0, 1, 1)));
        _lipids.Add(CreateLipid(new Vector3(0, 2, 0)));

        foreach (PhysicsObject physicsObject in _lipids)
            _softBodySimulation.AddNode(physicsObject);
        
        _softBodySimulation.AddJoint(CreateJoint(_lipids[0], _lipids[1], false));
        _softBodySimulation.AddJoint(CreateJoint(_lipids[0], _lipids[2], false));
        _softBodySimulation.AddJoint(CreateJoint(_lipids[0], _lipids[3], false));
        _softBodySimulation.AddJoint(CreateJoint(_lipids[1], _lipids[2], false));
        _softBodySimulation.AddJoint(CreateJoint(_lipids[3], _lipids[2], false));
        _softBodySimulation.AddJoint(CreateJoint(_lipids[3], _lipids[1], false));
        
        _softBodySimulation.Enabled = true;
        _softBodySimulation.ShowDebug = _showDebug;
    }

    private void FixedUpdate()
    {
        foreach (PhysicsObject physicsObject in _lipids)
            physicsObject.AddForce(_gravity * Vector3.down);
        _softBodySimulation.Update();
    }

    private SpringJoint CreateJoint(PhysicsObject fst, PhysicsObject snd, bool isDiagonal) =>
        new SpringJoint(fst, snd, _profile.SpringConstant, _profile.RestLength,
            _profile.MaxLength, _profile.MinLength, _profile.Damp, true, isDiagonal);
    
    private PhysicsObject CreateLipid(Vector3 position) =>
        new PhysicsObject(_profile.LipidMass, 0f, position, Vector3.zero, Instantiate(_prefab,transform));
}