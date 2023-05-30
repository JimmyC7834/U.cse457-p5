using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dMembraneSimulation : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    [SerializeField] private GameObject _prefab;

    [SerializeField] private bool _showDebug;
    [SerializeField] private Vector2Int _membraneSize;
    [SerializeField] private float _spacing;
    private PhysicsObject[] _lipids;
    private PhysicsObject[] _lipids2;
    private SoftBodySimulation _softBodySimulation;

    public int TotalLipidCount => _membraneSize.x * _membraneSize.y;

    private void Awake()
    {
        _softBodySimulation = new SoftBodySimulation();
        _lipids = new PhysicsObject[TotalLipidCount];
        _lipids2 = new PhysicsObject[TotalLipidCount];
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

    private SpringJoint CreateMembraneJoint(PhysicsObject fst, PhysicsObject snd) =>
        new SpringJoint(fst, snd, _profile.MemSpringConstant, _profile.MemRestLength,
            _profile.MemMaxLength, _profile.MemMinLength, _profile.MemDamp, true);

    private PhysicsObject CreateLipid(Vector3 position)
    {
        PhysicsObject node = new PhysicsObject(_profile.LipidMass, 0f, position, Vector3.zero, Instantiate(_prefab, transform));
        return node;
    }

    private void InitMembrane()
    {
        // add nodes
        float root3 = Mathf.Sqrt(3); // square root of 3
        float d = _spacing * root3; // distance between adjacent membranes
        Vector3 offset = new Vector3(-(_membraneSize.x - 1) * d / 2, 0, -(_membraneSize.y - 1) * _spacing / 2);

        for (int i = 0; i < _membraneSize.x; i++)
        {
            for (int j = 0; j < _membraneSize.y; j++)
            {
                // first membrane sheet
                int index = i * _membraneSize.y + j;
                Vector3 pos = new Vector3(i * d,
                             transform.position.y,
                             j * d) + offset;

                PhysicsObject node = CreateLipid(pos);
                _softBodySimulation.AddNode(node);
                _lipids[index] = node;

                // Second membrane sheet
                pos.y -= 1;

                PhysicsObject node2 = CreateLipid(pos);
                _softBodySimulation.AddNode(node2);
                _lipids2[index] = node2;
            }
        }

        ChangeLipids2ColorToOrange();

        // add bonds
        for (int i = 0; i < _membraneSize.x; i++)
        {
            for (int j = 0; j < _membraneSize.y; j++)
            {
                int index = i * _membraneSize.y + j;

                _softBodySimulation.AddJoint(CreateMembraneJoint(
                        _lipids[index],
                        _lipids2[index]
                    ));

                // add bonds
                if (j < _membraneSize.y - 1)
                {
                    _softBodySimulation.AddJoint(CreateJoint(
                            _lipids[index],
                            _lipids[index + 1]
                        ));
                    _softBodySimulation.AddJoint(CreateJoint(
                            _lipids2[index],
                            _lipids2[index + 1]
                        ));
                }


                if (i < _membraneSize.x - 1)
                {
                    _softBodySimulation.AddJoint(CreateJoint(
                        _lipids[index],
                        _lipids[index + _membraneSize.y]
                    ));

                    _softBodySimulation.AddJoint(CreateJoint(
                        _lipids2[index],
                        _lipids2[index + _membraneSize.y]
                    ));
                }

                if (i % 2 == 0)
                {
                    if (j < _membraneSize.y - 1 && i < _membraneSize.x - 1)
                    {

                        _softBodySimulation.AddJoint(CreateJoint(
                            _lipids[index],
                            _lipids[index + _membraneSize.y + 1]
                        ));

                        _softBodySimulation.AddJoint(CreateJoint(
                            _lipids2[index],
                            _lipids2[index + _membraneSize.y + 1]
                        ));
                    }

                }
                else
                {
                    if (j > 0 && i < _membraneSize.x - 1)
                    {
                        _softBodySimulation.AddJoint(CreateJoint(
                            _lipids[index],
                            _lipids[index + _membraneSize.y - 1]
                        ));
                        _softBodySimulation.AddJoint(CreateJoint(
                            _lipids2[index],
                            _lipids2[index + _membraneSize.y - 1]
                        ));
                    }

                }
            }
        }
    }

    private void ChangeLipids2ColorToOrange()
    {
        foreach (PhysicsObject lipid in _lipids2)
        {
            Renderer renderer = lipid.Rigidbody.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material orangeMaterial = new Material(Shader.Find("Standard"));
                orangeMaterial.color = new Color(1f, 0.5f, 0f);
                renderer.material = orangeMaterial;
            }
        }
    }
}
