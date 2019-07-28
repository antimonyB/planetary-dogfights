Shader "Custom/SurfaceShader"
{
    Properties
    {
		_elevationMinMax("Elevation Min Max", Vector) = (0,0,0,0)
		[NoScaleOffset] _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _Water("Water (RGB)", 2D) = "white" {}
		[NoScaleOffset] _Sand("Sand (RGB)", 2D) = "white" {}
		[NoScaleOffset] _Grass("Grass (RGB)", 2D) = "white" {}
		[NoScaleOffset] _Rocky("Rocky (RGB)", 2D) = "white" {}
		[NoScaleOffset] _Sandrock("Sandy Rock (RGB)", 2D) = "white" {}
		[NoScaleOffset] _Rocks("Rocks (RGB)", 2D) = "white" {}
		[NoScaleOffset] _Snow("Snow (RGB)", 2D) = "white" {}
	}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Water;
		sampler2D _Sand;
		sampler2D _Grass;
		sampler2D _Rocky;
		sampler2D _Sandrock;
		sampler2D _Rocks;
		sampler2D _Snow;
		float2 _elevationMinMax;

        struct Input
        {
            float2 texuv; //First uv channel for colour (MainTex).
			float4 xyze; //Second and third uv channel for texture. (Stores position and elevation).
			float3 worldNormal; //Vertex normal used to assign texture based on orientation.
        };


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.texuv = v.texcoord.xy;
			o.xyze = float4(v.texcoord1.x, v.texcoord1.y, v.texcoord2.x, v.texcoord2.y);
			//o.ze = v.texcoord2.xy;
		}
		float blend(float val, float min, float max, float blendMin, float blendMax) {
			return saturate((val - min + blendMin/2) / blendMin) * saturate((-val + max + blendMax/2) / blendMax);
		}
		float rand2(float2 coords) {
			return frac(sin(dot(coords, float2(12.9898, 78.233))) * 43758.5453);
		}
		half2 rotate(half2 samp) {
			// calculate rotation matrix parameters from the original UV data
			half r = (round(rand2(floor(samp)) * 3));
			half m1 = ((r - 1)*(3 - r)) / min(r - 3, -1);
			half m2 = (r*(2 - r)) / max(r, 1);
			half m3 = (r*(r - 2)) / max(r, 1);
			half m4 = ((3 - r)*(r - 1)) / min(r - 3, -1);

			// rotate texture UVs based on the calculated rotation matrix parameters
			samp -= 0.5;
			samp = mul(samp, float2x2(m1, m2, m3, m4));
			samp.xy += 0.5;
			return samp;
		}
		void surf (Input IN, inout SurfaceOutput o)
        {
			float2 uv = IN.texuv;
			float4 xyze = IN.xyze;
			float3 norm = (abs(IN.worldNormal));
			//float2 uvze = IN.ze;
			uv.x = ((uv.x/(_elevationMinMax.y*1.3))/2 + 0.5);

			float sand = blend(uv.x, 0.5, 0.505, 0.002, 0.02);
			float grass = blend(uv.x, 0.505, 0.57, 0.02, 0.05);
			float rocky = blend(uv.x, 0.57, 0.6, 0.05, 0.05)*0.8;
			float rocks = blend(uv.x, 0.6, 0.91, 0.05, 0.06);
			float snow = blend(uv.x, 0.91, 2, 0.06, 0.02)*0.9;
			// Albedo comes from a texture tinted by color
			float4 zProject = (tex2D(_Water, rotate(xyze.xy * 50)) * (saturate(pow((uv.x -0.395) * 8+0.15, 8)-0.15 - 0.5) + 0.5) * 1.2*(saturate(pow((uv.x -0.395) * 8+0.1, 8)+0.6)) * saturate((-uv.x+0.5+0.001)/0.002) + tex2D(_Sand, rotate(xyze.xy * 5000)) *1 * sand + tex2D(_Grass, rotate(xyze.xy * 5000)) * grass + tex2D(_Rocky, rotate(xyze.xy * 2000)) * rocky + tex2D(_Rocks, rotate(xyze.xy * 1000)) * rocks + tex2D(_Snow, rotate(xyze.xy * 1000)) * snow) * norm.z;
			float4 yProject = (tex2D(_Water, rotate(xyze.xz * 50)) * (saturate(pow((uv.x -0.395) * 8+0.15, 8)-0.15 - 0.5) + 0.5) * 1.2*(saturate(pow((uv.x -0.395) * 8+0.1, 8)+0.6)) * saturate((-uv.x+0.5+0.001)/0.002) + tex2D(_Sand, rotate(xyze.xz * 5000)) *1 * sand + tex2D(_Grass, rotate(xyze.xz * 5000)) * grass + tex2D(_Rocky, rotate(xyze.xz * 2000)) * rocky + tex2D(_Rocks, rotate(xyze.xz * 1000)) * rocks + tex2D(_Snow, rotate(xyze.xz * 1000)) * snow) * norm.y;
			float4 xProject = (tex2D(_Water, rotate(xyze.yz * 50)) * (saturate(pow((uv.x -0.395) * 8+0.15, 8)-0.15 - 0.5) + 0.5) * 1.2*(saturate(pow((uv.x -0.395) * 8+0.1, 8)+0.6)) * saturate((-uv.x+0.5+0.001)/0.002) + tex2D(_Sand, rotate(xyze.yz * 5000)) *1 * sand + tex2D(_Grass, rotate(xyze.yz * 5000)) * grass + tex2D(_Rocky, rotate(xyze.yz * 2000)) * rocky + tex2D(_Rocks, rotate(xyze.yz * 1000)) * rocks + tex2D(_Snow, rotate(xyze.yz * 1000)) * snow) * norm.x;
			fixed4 c = /*tex2D(_MainTex, uv)/5 + */xProject + yProject + zProject;
			//fixed4 c = tex2D(_Grass, rotate(xyze*5000));
			o.Albedo = c.rgb;
			o.Specular = 0;
			o.Gloss = 0;
			o.Emission = 0;
			o.Alpha = 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
