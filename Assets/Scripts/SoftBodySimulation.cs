using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SoftBodySimulation
{
    private List<SpringJoint> _joints;
    private List<PhysicsObject> _nodes;

    public bool ShowDebug = false;
    public bool Enabled = false;

    public SoftBodySimulation()
    {
        _joints = new List<SpringJoint>();
        _nodes = new List<PhysicsObject>();
    }
    
    public void Update()
    {
        if (!Enabled) return;
        
        // compute all the spring joint forces
        foreach (SpringJoint joint in _joints)
            ComputeJointForce(joint, ShowDebug);

        // update all nodes' physics
        foreach (PhysicsObject node in _nodes)
            node.ComputePhysics();
        
        // solve for each constraints
        foreach (SpringJoint joint in _joints)
            SolveDistanceConstraint(joint);

        // compute move velocity
        foreach (PhysicsObject node in _nodes)
            ComputeMoveVelocity(node);
    }

    public void AddJoint(SpringJoint joint)
    {
        if (_joints.Contains(joint)) return;
        _joints.Add(joint);
    }
    
    public void AddNode(PhysicsObject node)
    {
        if (_nodes.Contains(node)) return;
        _nodes.Add(node);
    }

    public static void ComputeJointForce(SpringJoint joint, bool showDebug)
    {
        if (!joint.Enabled) return;
        
        float k = joint.K;
        float restLength = joint.RestLength;

        PhysicsObject fst = joint.Fst;
        PhysicsObject snd = joint.Snd;
        float damp = joint.Damp;
        bool isBetweenMembrane = joint.IsBetweenMembrane;

        float dist = Vector3.Distance(snd.Position, fst.Position);
        Vector3 dir = (snd.Position - fst.Position).normalized;

        if (isBetweenMembrane)
        {
            k *= 2;
        }

        // contraction force of each end spring 
        float forceMag = k * (dist - restLength) * 0.5f;
        
        Vector3 f = forceMag * dir;
        
        // apply half of the total force to each end
        fst.AddForce(f - damp * fst.Velocity);
        snd.AddForce(-f - damp * snd.Velocity);
        
        if (showDebug) Debug.DrawLine(fst.Position, snd.Position);
    }

    public static void SolveDistanceConstraint(SpringJoint joint)
    {
        if (!joint.Enabled) return;
        
        float maxDist = joint.MaxDist;
        float minDist = joint.MinDist;
        PhysicsObject fst = joint.Fst;
        PhysicsObject snd = joint.Snd;
        float dist = Vector3.Distance(fst.Position, snd.Position);

        if (dist > maxDist)
        {
            Vector3 dir = (snd.Position - fst.Position).normalized;
            float dD = dist - maxDist;
            Vector3 dx = dD * -.5f * dir;
            fst.Rigidbody.position += -dx;
            snd.Rigidbody.position += dx;
        }

        if (dist < minDist)
        {
            Vector3 dir = (snd.Position - fst.Position).normalized;
            float dD = dist - minDist;
            Vector3 dx = dD * -.5f * dir;
            fst.Rigidbody.position += dx;
            snd.Rigidbody.position += -dx;
        }
    }

    public static void ComputeMoveVelocity(PhysicsObject node)
    {
        node.Rigidbody.velocity = (node.Position - node.PrevPosition);
    }
}

public struct SpringJoint
{
    public bool Enabled { get; private set; }
    public float K { get; private set; }
    public float RestLength { get; private set; }
    public float MaxDist { get; private set; }
    public float MinDist { get; private set; }
    public float Damp { get; private set; }
    public PhysicsObject Fst { get; private set; }
    public PhysicsObject Snd { get; private set; }
    public bool IsBetweenMembrane { get; private set; }

    public SpringJoint(
        PhysicsObject fst, PhysicsObject snd, float k, float restLength,
        float maxDist, float minDist, float damp, bool enabled, bool isBetweenMembrane)
    {
        Assert.IsTrue(maxDist >= minDist);
        
        Fst = fst;
        Snd = snd;
        K = k;
        RestLength = restLength;
        MaxDist = maxDist;
        MinDist = minDist;
        Damp = damp;
        Enabled = enabled;
        IsBetweenMembrane = isBetweenMembrane;
    }
}

public class PhysicsObject
{
    public float Mass;
    public float InverseMass;
    public float Scale;
    public Vector3 PrevPosition;
    public Vector3 Position  => Rigidbody.position;
    public Vector3 Velocity => Rigidbody.velocity;
    public Vector3 Acceleration;
    public Rigidbody Rigidbody;
    
    public PhysicsObject(float mass, float scale, Vector3 position, Vector3 velocity, GameObject obj) {
        Mass = mass;
        InverseMass = 1f / mass;
        Scale = scale;
        PrevPosition = position;
        Rigidbody = obj.GetComponent<Rigidbody>();
        Rigidbody.position = position;
        Rigidbody.velocity = velocity;
        // PhysicsSimulation.SetWorldScale(obj.transform, new Vector3(scale, scale, scale));
    }

    public void ComputePhysics()
    {
        PrevPosition = Position;
        Rigidbody.velocity += Acceleration;
        Rigidbody.position += Velocity;
        Acceleration = Vector3.zero;

        // Update the transform of the actual game object
        // Rigidbody.position = Position;
    }

    public void AddForce(Vector3 f)
    {
        Acceleration += InverseMass * f;
    }
}