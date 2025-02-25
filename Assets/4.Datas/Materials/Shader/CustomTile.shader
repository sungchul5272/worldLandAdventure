Shader "Custom/CustomTile"
{
    Properties
    {
        _TileColor ("Tile Color", Color) = (1,1,1,1)
        _BorderColor ("Border Color", Color) = (0,0,0,1)
        _BorderWidth ("Border Width", Range(0,0.1)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _TileColor; // **셰이더에서 사용할 타일 색상**
            float4 _BorderColor;
            float _BorderWidth;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float border = step(_BorderWidth, i.uv.x) * step(_BorderWidth, i.uv.y) * 
                               step(i.uv.x, 1.0 - _BorderWidth) * step(i.uv.y, 1.0 - _BorderWidth);
                
                return lerp(_BorderColor, _TileColor, border); // **타일 색상 적용**
            }
            ENDCG
        }
    }
}
