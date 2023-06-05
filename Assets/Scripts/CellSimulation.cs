using UnityEngine;
using System.Collections.Generic;

public class CellSimulation : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    [SerializeField] private GameObject _prefab;

    [SerializeField] private bool _showDebug;
    // Set _radius to 200
    [SerializeField] private int _radius;
    // Set _totalPoints to 8000000
    [SerializeField] private int _totalPoints;
    // Set _spacing to 1
    [SerializeField] private float _spacing;
    // Set _bondRadius to 0.17
    [SerializeField] private float _bondRadius;
    // Constant to tweak the amount of internal pressure there is, set to 0.5
    [SerializeField] private float _pressure;
    // toggle gravity
    [SerializeField] private bool _gravity;
    // toggle gravity value, set to -0.1
    [SerializeField] private float _gravityVal;
    
    private PhysicsObject[] _lipids;
    private SoftBodySimulation _softBodySimulation;
    // normal of each lipid when it is at (0,0) and the cell is a perfect sphere
    private List<Vector3> _normals;
   
    public int TotalLipidCount => _radius*_radius*_radius;
    // Volume and Surface Area of perfect sphere
    public float Volume => (4.0f/3.0f)*Mathf.PI*(_radius*_radius*_radius);
    public float Area => Mathf.PI*(_radius*_radius);

    private void Awake()
    {
        _normals = new List<Vector3>();
        _softBodySimulation = new SoftBodySimulation();
        _lipids = new PhysicsObject[TotalLipidCount];
    }

    private void Start()
    {
        InitSphere();
        
       
        foreach (PhysicsObject node in _softBodySimulation._nodes) {
            _normals.Add(node.Position.normalized);
        }
    
        _softBodySimulation.Enabled = true;
        _softBodySimulation.ShowDebug = _showDebug;
    }

    private void FixedUpdate()
    {
        _softBodySimulation.Update();
        int i = 0;
        // Simulate internal pressure so cell does not deflate on contact with other objects
        // inspiration from: https://www.researchgate.net/publication/228574502_How_to_implement_a_pressure_soft_body_model
        foreach (PhysicsObject node in _softBodySimulation._nodes) {
            // Temp: 22C (room temp) Ideal Gas Constant: 8.314
            node.AddForce(((Area*22.0f*8.314f)/Volume)*_normals[i]*_pressure);
            if (_gravity) 
                node.AddForce(new Vector3 (0f, _gravityVal, 0f));
            i++;
        }
    }

    private SpringJoint CreateJoint(PhysicsObject fst, PhysicsObject snd) =>
        new SpringJoint(fst, snd, _profile.SpringConstant, Vector3.Distance(fst.Position, snd.Position),
            _profile.MaxLength, _profile.MinLength, _profile.Damp, true);
    
    private PhysicsObject CreateLipid(Vector3 position) =>
        new PhysicsObject(_profile.LipidMass, 0f, position, Vector3.zero, Instantiate(_prefab,transform));

    private void InitSphere()
    {
        // The following is an algorithm I got from a paper on how to equally distribute
        // points on a sphere. https://www.cmu.edu/biolphys/deserno/pdf/sphere_equi.pdf
        int r = _radius;
        int count = 0;
        float alpha = (4*Mathf.PI*r*r)/_totalPoints;
        float d = Mathf.Sqrt(alpha);
        int M_v = (int) Mathf.Round(Mathf.PI/d);
        float d_v = Mathf.PI / M_v;
        float d_y = alpha / d_v;
        // This is used for getting nodes from positions when creating the joints later
        Dictionary<Vector3, PhysicsObject> nodeMap = new Dictionary<Vector3, PhysicsObject>();
        for (int i = 0; i < M_v; i++) {
            float v = Mathf.PI*(i + 0.5f) / M_v;
            int M_y = (int) Mathf.Round(2*Mathf.PI*Mathf.Sin(v)/d_y);
            for (int j = 0; j < M_y; j++) {
                float y = 2*Mathf.PI*j/M_y;
                Vector3 pos = new Vector3(Mathf.Sin(v)*Mathf.Cos(y), Mathf.Sin(v)*Mathf.Sin(y), Mathf.Cos(v));
                PhysicsObject node = CreateLipid(pos);
                _softBodySimulation.AddNode(node);
                nodeMap[pos] = node;
                _lipids[count] = node;
                count++;
            }
        }


        for (int index = 1; index < count; index++)
        {
            // Add bonds
            Collider[] collisions = Physics.OverlapSphere(_lipids[index].Position, _bondRadius);
            foreach (Collider col in collisions) {
                Rigidbody closeParticle = col.GetComponent<Rigidbody>();
                _softBodySimulation.AddJoint(CreateJoint(
                        // Curr
                        _lipids[index],
                        // Particle in radius
                        nodeMap[closeParticle.position]
                ));
            }
        }      
    }
}