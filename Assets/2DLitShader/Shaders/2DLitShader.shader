Shader "2D/2DLitShader" {
	Properties{
		_Color("Main Color", Color) = (0.1,0.1,0.1,0)
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

		SubShader{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			Cull Off
			Fog { Mode Off }

		// FIX: enable proper alpha blending
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		#pragma target 3.0

		// FIX: tell surface shader to use alpha blending
		#pragma surface surf SimpleLambert alpha:blend

		float4 _Color;
		sampler2D _MainTex;

		half4 LightingSimpleLambert(SurfaceOutput s, half3 lightDir, half atten) {
			half NdotL = dot(s.Normal, lightDir);
			half diff = NdotL + 1.0;

			float4 ambient = UNITY_LIGHTMODEL_AMBIENT * (2 * _Color);
			half3 lc = _LightColor0.rgb * 4;

			half4 c;
			c.rgb = s.Albedo * ambient.rgb * lc * (diff * atten * 2);

			// preserve alpha
			c.a = s.Alpha;

			return c;
		}

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			float4 tex = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = tex.rgb;

			// FIX: pass alpha to lighting
			o.Alpha = tex.a;
		}
		ENDCG
	}
}