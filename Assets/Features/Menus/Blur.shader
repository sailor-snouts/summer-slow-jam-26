Shader "Hidden/JamTemplate/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        float _BlurSize;

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        v2f vert (appdata_img v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            return o;
        }

        static const float weights[5] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };

        fixed4 blur (float2 uv, float2 dir)
        {
            fixed4 col = tex2D(_MainTex, uv) * weights[0];
            [unroll]
            for (int i = 1; i < 5; i++)
            {
                float2 offset = dir * i * _BlurSize;
                col += tex2D(_MainTex, uv + offset) * weights[i];
                col += tex2D(_MainTex, uv - offset) * weights[i];
            }
            return col;
        }
        ENDCG

        Pass // 0 - horizontal
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            fixed4 frag (v2f i) : SV_Target
            {
                return blur(i.uv, float2(_MainTex_TexelSize.x, 0));
            }
            ENDCG
        }

        Pass // 1 - vertical
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            fixed4 frag (v2f i) : SV_Target
            {
                return blur(i.uv, float2(0, _MainTex_TexelSize.y));
            }
            ENDCG
        }
    }
}
