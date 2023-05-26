using UnityEngine;

public class CellSimulation : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    [SerializeField] private GameObject _prefab;

    [SerializeField] private bool _showDebug;
    [SerializeField] private int _radius;
    [SerializeField] private float _spacing;
    private PhysicsObject[] _lipids;
    private SoftBodySimulation _softBodySimulation;

    public int TotalLipidCount => _radius*_radius*_radius;

    private void Awake()
    {
        _softBodySimulation = new SoftBodySimulation();
        _lipids = new PhysicsObject[TotalLipidCount];
    }

    private void Start()
    {
        InitSphere();
        _softBodySimulation.Enabled = false;
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

    private void InitSphere()
    {
        int r = _radius;
        // add nodes
        // Vector3 offset = new Vector3(-_spacing, 0, -_spacing);
        Vector3 baseV = new Vector3(r, r, r);
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < r; j++)
            {
                for (int k = 0; k < r; k++) {
                    int index = (i * r * r) + (j * r) + k;
                    Vector3 pos = new Vector3(
                        i - r/2,
                        j - r/2,
                        k - r/2);
                    pos.Normalize();
                    pos.Scale(baseV);
                    PhysicsObject node = CreateLipid(pos);
                    _softBodySimulation.AddNode(node);
                    _lipids[index] = node;
                }
            }
        }
        
        // add bonds
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < r; j++)
            {
                for (int k = 0; k < r; k++) {
                    int index = (i * i * r) + (j * r) + k;
                    // add bonds
                    if (j < r - 1)
                        _softBodySimulation.AddJoint(CreateJoint(
                                _lipids[index],
                                _lipids[index + 1]
                            ));
                    
                    if (i < r - 1)
                        _softBodySimulation.AddJoint(CreateJoint(
                            _lipids[index],
                            _lipids[index]
                        ));

                    if (k < r - 1)
                        _softBodySimulation.AddJoint(CreateJoint(
                            _lipids[index],
                            _lipids[index + (j*r)]
                        ));
                    
                    if (j < r - 1 && i < r - 1)
                        _softBodySimulation.AddJoint(CreateJoint(
                            _lipids[index],
                            _lipids[index + 1]
                        ));
                    
                    if (j > 0 && i < r - 1)
                        _softBodySimulation.AddJoint(CreateJoint(
                            _lipids[index],
                            _lipids[index + r - 1]
                        ));
                    
                    if (k > 0 && i < r - 1)
                        _softBodySimulation.AddJoint(CreateJoint(
                            _lipids[index],
                            _lipids[index + r - 1]
                        ));
                }                
            }
        }
    }
}