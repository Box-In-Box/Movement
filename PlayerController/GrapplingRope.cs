using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    
    private  List<Spring> springs;
    public List<LineRenderer> lineRenderers;
    public Swing swing;
    private List<Vector3> currentGrapplePositions;
    public int quality;
    public float damper;
    public float strength;
    public float velocity;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve affectCurve;
    
    private void Awake()
    {
        springs = new List<Spring>();
        lineRenderers = new List<LineRenderer>();
        currentGrapplePositions = new List<Vector3>();

        for (int i = 0; i < swing.amountOfSwingPoints; i++) {
            springs.Add(new Spring());
            springs[i].SetTarget(0);

            lineRenderers.Add(swing.lineRenderers[i]);

            currentGrapplePositions.Add(Vector3.zero);
        }
    }
    private void LateUpdate()
    {
        for (int i = 0; i < swing.amountOfSwingPoints; i++) {
            DrawRope(i);
        }
    }
    
    private void DrawRope(int index)
    {
        if (!swing.swingsActive[index]) {
            currentGrapplePositions[index] = swing.swingTips[index].position;
            springs[index].Reset();
            
            if (lineRenderers[index].positionCount > 0)
                lineRenderers[index].positionCount = 0;
            return;
        }

        if (lineRenderers[index].positionCount == 0) {
            springs[index].SetVelocity(velocity);
            lineRenderers[index].positionCount = quality + 1;
        } 

        springs[index].SetDamper(damper);
        springs[index].SetStrength(strength);
        springs[index].Update(Time.deltaTime);

        var grapplePoint = swing.swingPoints[index];
        var gunTipPosition = swing.swingTips[index].position;
        var up = Quaternion.LookRotation(grapplePoint - gunTipPosition).normalized * Vector3.up;

        currentGrapplePositions[index]  = Vector3.Lerp(currentGrapplePositions[index] , swing.swingPoints[index], Time.deltaTime * 12f);

        for (int i = 0; i < quality + 1; i++) {
            var delta = i / (float)quality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * springs[index].Value * affectCurve.Evaluate(delta);
            lineRenderers[index].SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePositions[index] , delta) + offset);
        } 
    }
    
}
