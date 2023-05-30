using UnityEngine;

[CreateAssetMenu(menuName = "SimulationProfile")]
public class SimulationProfile : ScriptableObject
{
    [Header("Spring Joint Settings")]
    [SerializeField] private float _springConstant;
    [SerializeField] private float _restLength;
    [Range(0f, 10f)]
    [SerializeField] private float _damp;
    [SerializeField] private float _maxLength;
    [SerializeField] private float _minLength;

    [Header("Membrane Joint Settings")]
    [SerializeField] private float _memSpringConstant;
    [SerializeField] private float _memRestLength;
    [Range(0f, 10f)]
    [SerializeField] private float _memDamp;
    [SerializeField] private float _memMaxLength;
    [SerializeField] private float _memMinLength;

    [Header("Simulation Settings")]
    // [SerializeField] private float _lipidRadius;
    [SerializeField] private float _lipidMass;
    [Range(0f, 1f)]
    [SerializeField] private float _velocityRestitution;
    
    public float LipidMass => _lipidMass;
    public float RestLength => _restLength;
    public float SpringConstant => _springConstant;
    public float VelocityRestitution => _velocityRestitution;
    public float Damp => _damp;
    public float MaxLength => _maxLength;
    public float MinLength => _minLength;

    public float MemRestLength => _memRestLength;
    public float MemSpringConstant => _memSpringConstant;
    public float MemDamp => _memDamp;
    public float MemMaxLength => _memMaxLength;
    public float MemMinLength => _memMinLength;

    // [SerializeField] private AnimationCurve _headAttractionForceCurve;
    // [SerializeField] private AnimationCurve _tailAttractionForceCurve;
    // public float HeadAttractionForce(float dist) => _attractionForce * _headAttractionForceCurve.Evaluate(dist);
    // public float TailAttractionForce(float dist) => _attractionForce * _tailAttractionForceCurve.Evaluate(dist);
}
