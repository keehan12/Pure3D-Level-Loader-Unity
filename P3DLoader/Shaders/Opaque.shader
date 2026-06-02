Shader "Custom/Opaque"
{
    Properties
    {
        _Color   ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }
	
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
		
        LOD      200
        Lighting Off
		Cull Off
 
        CGPROGRAM
        #pragma surface surf Lambert
 
        sampler2D _MainTex;
        fixed4 _Color;
 
        struct Input
        {
            float2 uv_MainTex;
			float4 vertexColor : COLOR;
        };
 
        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb * IN.vertexColor;
        }
		
        ENDCG
    }
}