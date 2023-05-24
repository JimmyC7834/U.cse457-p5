using UnityEngine;

[CreateAssetMenu(menuName = "SimulationProfile")]
public class SimulationProfile : ScriptableObject
{
    [Header("Spring Joint Settings")]
    [SerializeField] private float _springConstant;
    [SerializeField] private float _eqmDist;
    [SerializeField] private float _damping;

    [Header("Simulation Settings")]
    [SerializeField] private float _lipidRadius;
    [SerializeField] private float _lipidMass;
    [Range(0f, 1f)]
    [SerializeField] private float _velocityRestitution;

    public float LipidRadius => _lipidRadius;
    public float LipidMass => _lipidMass;
    public float EqmDist => _eqmDist;
    public float SpringConstant => _springConstant;
    public float VelocityRestitution => _velocityRestitution;
    public float Damping => _damping;

    // [SerializeField] private AnimationCurve _headAttractionForceCurve;
    // [SerializeField] private AnimationCurve _tailAttractionForceCurve;
    // public float HeadAttractionForce(float dist) => _attractionForce * _headAttractionForceCurve.Evaluate(dist);
    // public float TailAttractionForce(float dist) => _attractionForce * _tailAttractionForceCurve.Evaluate(dist);
}
