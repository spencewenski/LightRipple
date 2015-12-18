Shader "Custom/LightRipple" {
	Properties{
		_RippleColor("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha     // Alpha blending
		//Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _RippleColor;

		// 'constants'
		float _MaxRadius_c;
		float _RippleWidth_c;
		float _RippleSpacing_c;
		float _NumConcentricRipples_c;
		float _RippleAlpha_c;

		// ripple data
		#define MAX_RIPPLES 10
		int _RippleCount;
		float3 _RippleCenter[MAX_RIPPLES];
		float2 _CurrentMaxRadius[MAX_RIPPLES];

		// returns 0 if the objectPosition is inside a ripple, -1 otherwise
		int insideRipple(float3 objectPosition, float3 rippleCenter, float rippleDistance) {
			float d = distance(rippleCenter, objectPosition);
			if (d > _MaxRadius_c) {
				return -1;
			}
			for (int i = 0; i < _NumConcentricRipples_c * 2; ++i) {
				int modulo = int(fmod(float(i), 2));
				if (modulo == 0 && d > rippleDistance) {
					return -1;
				}
				else if (modulo == 1 && d > rippleDistance) {
					return 0;
				}
				rippleDistance -= modulo == 0 ? _RippleWidth_c : _RippleSpacing_c;
			}
			return -1;
		}

		int ripple(float3 worldPos) {
			int insideCount = 0;
			for (int i = 0; i < _RippleCount; i++) {
				if (insideRipple(worldPos, _RippleCenter[i], _CurrentMaxRadius[i].x) == 0) {
					++insideCount;
				}
			}
			return insideCount;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			int insideCount = ripple(IN.worldPos);
			if (insideCount == 0) {
				clip(-1);
				return;
			}
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _RippleColor;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = clamp(0, 1, _RippleAlpha_c * insideCount);
		}
		ENDCG
	}
	//FallBack "Diffuse"
	Fallback "Transparent/VertexLit"
}

