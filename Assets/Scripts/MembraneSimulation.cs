using System;
using System.Data;
using UnityEngine;

public class MembraneSimulation : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private GameObject _sphere;

    [SerializeField] private bool _showDebug;
    [SerializeField] private Vector2Int _membraneSize;
    [SerializeField] private float _spacing;
    [SerializeField] private float _wrapDist;
    [SerializeField] private float _g;
    [SerializeField] private float _breakHeight;
    [SerializeField] private Vector3 _breakPlanePosition;
    [SerializeField] private JointSettings _wrapJointSettings;
    
    private PhysicsObject[] _lipids;
    private SpringJoint[] _wrapJoints;
    private SoftBodySimulation _softBodySimulation;

    private bool _cut = false;

    public int TotalLipidCount => _membraneSize.x * _membraneSize.y;

    private void Awake()
    {
        _softBodySimulation = new SoftBodySimulation();
        _lipids = new PhysicsObject[TotalLipidCount + 1];
        _wrapJoints = new SpringJoint[TotalLipidCount];
    }

    private void Start()
    {
        InitMembrane();
        _softBodySimulation.ShowDebug = _showDebug;
        
        Invoke(nameof(StartSim), 2f);
    }

    private void StartSim()
    {
        _softBodySimulation.Enabled = true;
        _lipids[0].AddForce(_g * Vector3.down);
    }

    private void FixedUpdate()
    {
        if (!_softBodySimulation.Enabled) return;
        
        _lipids[0].AddForce(_g * Vector3.down);
        _softBodySimulation.Update();
        CheckAndEnableJoint();
    }

    private void CheckAndEnableJoint()
    {
        if (_sphere.transform.position.y > _breakHeight) return;
        if (_cut) return;
        _cut = true;
        float h = _breakPlanePosition.y;

        foreach (SpringJoint joint in _softBodySimulation.Joints)
        {
            float y1 = joint.Fst.Position.y;
            float y2 = joint.Snd.Position.y;
            if ((y1 > h && y2 < h) || (y1 < h && y2 > h))
            {
                joint.Enabled = false;
            }
            // if (y1 > h || y2 < h)
            // {
            //     joint.Enabled = false;
            // }
        }

        // int[] indices = new int[]
        // {
        //     167, 175,
        //     111, 196,
        //     112, 255,
        //     154, 274,
        //     175, 293,
        //     196, 292,
        //     255, 310,
        //     274, 290,
        //     293, 289,
        //     292, 268,
        //     310, 247,
        //     290, 226,
        //     289, 206,
        //     268, 187,
        //     247, 167,
        //     226, 112,
        //     206, 154,
        // };
        
        int[] indices = new int[]
        {
            215, 207,
            131, 291,
            253, 169,
            173, 249
        };

        foreach (SpringJoint joint in _wrapJoints)
        {
            if (joint.Distance < _wrapDist || joint.Snd.Position.y < h)
                joint.Enabled = true;
        }

        for (int i = 0; i < indices.Length; i += 2)
        {
            _softBodySimulation.AddJoint(CreateJoint(
                _lipids[indices[i]],
                _lipids[indices[i + 1]]
            ));
        }
    }

    private SpringJoint CreateJoint(PhysicsObject fst, PhysicsObject snd) =>
        new SpringJoint(fst, snd, _profile.MembraneJointSettings, true);

    private PhysicsObject CreateLipid(Vector3 position, int index)
    {
        GameObject obj = Instantiate(_prefab, transform);
        obj.name += index.ToString();
        return new PhysicsObject(_profile.LipidMass, 0f, position, Vector3.zero, obj);
    }

    private void InitMembrane()
    {
        // add nodes
        Vector3 offset = new Vector3(-_spacing * _membraneSize.x / 2, 0, -_spacing * _membraneSize.y / 2);

        PhysicsObject sphere = new PhysicsObject(_profile.LipidMass * 2f, 0f,
            2.5f * Vector3.up, Vector3.zero, _sphere);
        _softBodySimulation.AddNode(sphere);
        _lipids[0] = sphere;
        
        for (int i = 0; i < _membraneSize.x; i++)
        {
            for (int j = 0; j < _membraneSize.y; j++)
            {
                int index = i * _membraneSize.y + j;
                Vector3 pos = new Vector3(
                    i * _spacing,
                    transform.position.y,
                    _spacing * j) + offset;

                PhysicsObject node = CreateLipid(pos, index + 1);
                _softBodySimulation.AddNode(node);
                _lipids[index + 1] = node;

                SpringJoint wrapJoint = 
                    new SpringJoint(_lipids[0], node, _wrapJointSettings, false);;
                _wrapJoints[index] = wrapJoint;
                _softBodySimulation.AddJoint(wrapJoint);
            }
        }
        
        // add bonds
        for (int i = 0; i < _membraneSize.x; i++)
        {
            for (int j = 0; j < _membraneSize.y; j++)
            {
                int index = i * _membraneSize.y + j + 1;
                
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
                
                // if (j < _membraneSize.y - 1 && i < _membraneSize.x - 1)
                //     _softBodySimulation.AddJoint(CreateJoint(
                //         _lipids[index],
                //         _lipids[index + _membraneSize.y + 1]
                //     ));
                //
                // if (j > 0 && i < _membraneSize.x - 1)
                //     _softBodySimulation.AddJoint(CreateJoint(
                //         _lipids[index],
                //         _lipids[index + _membraneSize.y - 1]
                //     ));
                
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(Vector3.up * _breakPlanePosition.y, new Vector3(10, 0, 10));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.up * _breakHeight, new Vector3(3, 0, 3));
    }
}