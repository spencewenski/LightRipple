using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightRipple : MonoBehaviour {

    // ripple speed
    public float rippleSpeed = 2;

    // time between two ripples
    public float rippleTimeout = 1f;
    private float rippleTimeoutRemaining; // current time between two ripples remaining

    // ripple appearance
    public float rippleWidth = 1f;
    public float rippleSpacing = 2f;
    public float maxRadius = 1f;
    public int numConcentricRipples = 2;
    public float rippleAlpha = 1f;
    public Color rippleColor = Color.white;

    const int maxRipples = 10;
    private List<Ripple> ripples = new List<Ripple>();
    private int nextRipple = 0;
	private int rippleCount = 0;

    private Vector3 previousPosition;

    private Material material;

    class Ripple {
        private Vector3 rippleCenter;
        private float currentMaxRadius = 0f;
        private bool rippling = false;

        public bool Start(Vector3 rippleCenter_) {
            if (rippling) {
                return false;
            }
            rippleCenter = rippleCenter_;
            currentMaxRadius = 0f;
            rippling = true;
            return true;
        }

        public bool Update(float maxRippleDistance, float rippleSpeed, Vector3 deltaPosition) {
            if (!rippling) {
                return false;
            }
            currentMaxRadius += rippleSpeed * Time.deltaTime;
            rippleCenter += deltaPosition;
            return true;

        }

        private float getCurrentMinRadius(float rippleWidth, float rippleSpacing, int numConcentricRipples) {
            float minRadius = currentMaxRadius - (numConcentricRipples * rippleWidth) - ((numConcentricRipples - 1) * rippleSpacing);
            return Mathf.Max(minRadius, 0f);
        }

        // sets rippling to false if the last ripple has moved outside the maxRadius
        public void updateIsRippling(float maxRadius, float rippleWidth, float rippleSpacing,
                int numConcentricRipples) {
            if (!rippling) {
                return;
            }
            rippling = getCurrentMinRadius(rippleWidth, rippleSpacing, numConcentricRipples) <= maxRadius;
        }

        public float getCurrentMaxRadius() {
            return currentMaxRadius;
        }

        public bool isRippling() {
            return rippling;
        }

        public Vector3 getRippleCenter() {
            return rippleCenter;
        }
    }

    void Awake() {
        for (int i = 0; i < maxRipples; ++i) {
            ripples.Add(new Ripple());
        }
        material = GetComponent<Renderer>().material;
        previousPosition = transform.position;

        material.SetFloat("_MaxRadius_c", maxRadius);
        material.SetFloat("_RippleWidth_c", rippleWidth);
        material.SetFloat("_RippleSpacing_c", rippleSpacing);
        material.SetFloat("_NumConcentricRipples_c", numConcentricRipples);
        material.SetFloat("_RippleAlpha_c", rippleAlpha);
        material.SetVector("_RippleColor", rippleColor);
    }

    
    // Use this for initialization
    void Start () {
        
    }

    private bool updateShaderParams() {
        Vector3 deltaPosition = transform.position - previousPosition;
        previousPosition = transform.position;
        for (int i = 0; i < rippleCount; ++i) {
            if (!ripples[i].isRippling()) {
                continue;
            }
            ripples[i].Update(maxRadius, rippleSpeed, deltaPosition);
            ripples[i].updateIsRippling(maxRadius, rippleWidth, rippleSpacing, numConcentricRipples);

            material.SetVector("_CurrentMaxRadius" + i, new Vector2(ripples[i].getCurrentMaxRadius(), 0f));
            material.SetVector("_RippleCenter" + i, ripples[i].getRippleCenter());
        }
        return true;
    }

    // Update is called once per frame
    void Update () {
        updateShaderParams();
        rippleTimeoutRemaining = Utility.updateTimeRemaining(rippleTimeoutRemaining);
    }

	public void setRippleColor(Color col) {
		material.SetVector("_RippleColor", col);
	}

    private void startRipple(Vector3 collisionPosition) {
        if (!ripples[nextRipple].Start(collisionPosition)) {
            return;
        }
        material.SetVector("_RippleCenter" + nextRipple, ripples[nextRipple].getRippleCenter());
        nextRipple = (nextRipple + 1) % maxRipples;
		rippleCount = Mathf.Min(rippleCount + 1, maxRipples);
        material.SetInt("_RippleCount", rippleCount);
    }

	void OnCollisionEnter(Collision other) {
        if (rippleTimeoutRemaining > 0f) {
            return;
        }
        rippleTimeoutRemaining = rippleTimeout;
        startRipple(other.contacts[0].point);
    }
}
