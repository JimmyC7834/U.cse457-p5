using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SimulationProfile")]
public class SimulationProfile : ScriptableObject
{
    [Header("Spring Joint Settings")]
    [SerializeField] private JointSettings _membraneJointSettings;
    [SerializeField] private JointSettings _lipidJointSettings;

    [Header("Simulation Settings")]
    [SerializeField] private float _lipidMass;
    [Range(0f, 1f)]
    [SerializeField] private float _velocityRestitution;
    
    public float LipidMass => _lipidMass;
    public float VelocityRestitution => _velocityRestitution;

    public JointSettings MembraneJointSettings => _membraneJointSettings;
    public JointSettings LipidJointSettings => _lipidJointSettings;

    // [SerializeField] private AnimationCurve _headAttractionForceCurve;
    // [SerializeField] private AnimationCurve _tailAttractionForceCurve;
    // public float HeadAttractionForce(float dist) => _attractionForce * _headAttractionForceCurve.Evaluate(dist);
    // public float TailAttractionForce(float dist) => _attractionForce * _tailAttractionForceCurve.Evaluate(dist);
}

[Serializable]
public struct JointSettings
{
    public float RestLength => _restLength;
    public float SpringConstant => _springConstant;
    public float Damp => _damp;
    public float MaxLength => _maxLength;
    public float MinLength => _minLength;
    public float BreakLength => _breakLength;
    
    [SerializeField] private float _springConstant;
    [Range(0f, 5f)]
    [SerializeField] private float _restLength;
    [Range(0f, 2f)]
    [SerializeField] private float _damp;
    [Range(0f, 5f)]
    [SerializeField] private float _maxLength;
    [Range(0f, 5f)]
    [SerializeField] private float _minLength;
    [Range(0f, 5f)]
    [SerializeField] private float _breakLength;
}
