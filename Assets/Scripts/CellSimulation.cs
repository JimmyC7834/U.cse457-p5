using UnityEngine;
using System.Collections.Generic;

public class CellSimulation : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    [SerializeField] private GameObject _prefab;

    [SerializeField] private bool _showDebug;
    [SerializeField] private int _radius;
    [SerializeField] private int _totalPoints;
    [SerializeField] private float _spacing;
    [SerializeField] private float _bondRadius;
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
        _softBodySimulation.Enabled = true;
        _softBodySimulation.ShowDebug = _showDebug;
    }

    private void FixedUpdate()
    {
        _softBodySimulation.Update();
    }

    private SpringJoint CreateJoint(PhysicsObject fst, PhysicsObject snd) =>
        new SpringJoint(fst, snd, _profile.SpringConstant, Vector3.Distance(fst.Position, snd.Position),
            _profile.MaxLength, _profile.MinLength, _profile.Damp, true);
    
    private PhysicsObject CreateLipid(Vector3 position) =>
        new PhysicsObject(_profile.LipidMass, 0f, position, Vector3.zero, Instantiate(_prefab,transform));

    private void InitSphere()
    {
        // The following is an algorithm I got from a paper on how to equally distribute
        // points on a sphere.
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
            // Debug.Log(i);
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