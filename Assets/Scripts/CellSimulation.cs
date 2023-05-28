using UnityEngine;

public class CellSimulation : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    [SerializeField] private GameObject _prefab;

    [SerializeField] private bool _showDebug;
    [SerializeField] private int _radius;
    [SerializeField] private int _totalPoints;
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

    private void InitSphere()
    {
        int r = _radius;
        // add nodes
        // Vector3 offset = new Vector3(-_spacing, 0, -_spacing);
        // Vector3 baseV = new Vector3(Mathf.Sqrt(r), Mathf.Sqrt(r), Mathf.Sqrt(r));
        // for (int i = 0; i < r; i++)
        // {
        //     for (int j = 0; j < r; j++)
        //     {
        //         for (int k = 0; k < r; k++) {
        //             int index = (i * r * r) + (j * r) + k;
        //             Vector3 pos = new Vector3(
        //                 (i - r/2),
        //                 (j - r/2),
        //                 (k - r/2));
        //             pos.Normalize();
        //             pos.Scale(baseV);
        //             PhysicsObject node = CreateLipid(pos/(1/_spacing));
        //             _softBodySimulation.AddNode(node);
        //             _lipids[index] = node;
        //         }
        //     }
        // }

        // Expiremental Placement
        int count = 0;
        float alpha = (4*Mathf.PI*r*r)/_totalPoints;
        float d = Mathf.Sqrt(alpha);
        int M_v = (int) Mathf.Round(Mathf.PI/d);
        float d_v = Mathf.PI / M_v;
        float d_y = alpha / d_v;
        for (int i = 0; i < M_v; i++) {
            float v = Mathf.PI*(i + 0.5f) / M_v;
            int M_y = (int) Mathf.Round(2*Mathf.PI*Mathf.Sin(v)/d_y);
            for (int j = 0; j < M_y; j++) {
                // int index = (i * r) + j;
                float y = 2*Mathf.PI*j/M_y;
                Vector3 pos = new Vector3(Mathf.Sin(v)*Mathf.Cos(y), Mathf.Sin(v)*Mathf.Sin(y), Mathf.Cos(v));
                PhysicsObject node = CreateLipid(pos);
                _softBodySimulation.AddNode(node);
                _lipids[count] = node;
                // for (int k = 0; k < _totalPoints; k++) {
                //     float z = 2*k*(1/r) - r;
                //     float y = k*(1/(2*Mathf.PI));
                //     Vector3 pos = new Vector3(Mathf.Sqrt(r*r-z*z)*Mathf.Cos(y), Mathf.Sqrt(r*r-z*z)*Mathf.Sin(y), z);
                //     PhysicsObject node = CreateLipid(pos);
                //     _softBodySimulation.AddNode(node);
                //     _lipids[index] = node;
                // }
                count++;
            }
        }




        
        // add bonds
        // for (int i = 0; i < r; i++)
        // {
        //     for (int j = 0; j < r; j++)
        //     {
        //         int index = ;
        //         // add bonds
        //         if (j < r - 1)
        //             _softBodySimulation.AddJoint(CreateJoint(
        //                     _lipids[index],
        //                     _lipids[index + 1]
        //                 ));
                
        //         if (i < r - 1)
        //             _softBodySimulation.AddJoint(CreateJoint(
        //                 _lipids[index],
        //                 _lipids[index]
        //             ));
                
        //         if (j < r - 1 && i < r - 1)
        //             _softBodySimulation.AddJoint(CreateJoint(
        //                 _lipids[index],
        //                 _lipids[index + 1]
        //             ));
                
        //         if (j > 0 && i < r - 1)
        //             _softBodySimulation.AddJoint(CreateJoint(
        //                 _lipids[index],
        //                 _lipids[index + r - 1]
        //             ));             
        //         }
        // }


        for (int index = 1; index < count; index++)
        {
            // add bonds
            // if (index < count - 1)
            //     _softBodySimulation.AddJoint(CreateJoint(
            //             _lipids[index],
            //             _lipids[index + 1]
            //         ));
            
            // if (index < count - 1)
            //     _softBodySimulation.AddJoint(CreateJoint(
            //         _lipids[index],
            //         _lipids[index - 1]
            //     ));

            // if (index < count - 1)
            //     _softBodySimulation.AddJoint(CreateJoint(
            //         _lipids[index],
            //         _lipids[index + (count/2) % count]
            //     ));

            // if (index < count - 1)
            //     _softBodySimulation.AddJoint(CreateJoint(
            //         _lipids[index],
            //         _lipids[index]
            //     ));
        }    
    }
}