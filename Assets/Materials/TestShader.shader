// Name and where to find in inspector
Shader "Unlit/TestShader"
{
    // Assign external variables here
    Properties
    {
        // Getting external texture variable and setting default value
        //_MainTex ("Texture", 2D) = "white" {}
        segments ("Segments", Int) = 1
        pixelAmount ("Pixelation Amount", Int) = 1
    }
    // All shader code
    SubShader
    {
        Pass
        {
            // Start programming language of CG
            CGPROGRAM
            // Specify names of vertex and function shader, must be #pragma vertex / #praga fragment, but the name following each corrosponds to assigned function
            #pragma vertex vert
            #pragma fragment frag

            // Allow use of Unity specific functions
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // Data from mesh
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal :NORMAL;
            };
            

            float segments;
            int pixelAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 normal = round(i.normal * pixelAmount) / pixelAmount;
                float3 light = dot(lightDir, normal);
                return float4(round(light * segments) / segments, 1);
            }
            
            ENDCG
        }
    }
}
