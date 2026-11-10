Shader "UI/ScanTransition"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Start ("Start", Float) = -0.2
        _End ("End", Float) = -0.2
        _Slant ("Slant", Float) = 0.3
        _EdgeWidth ("Edge Width", Float) = 0.03
        _GridX ("Grid X", Float) = 32
        _GridY ("Grid Y", Float) = 18
        _GridThickness ("Grid Thickness", Float) = 0.06
        _DarkColor ("Dark Color", Color) = (0.04, 0.03, 0.09, 1)
        _GridColor ("Grid Color", Color) = (0, 0.9, 1, 0.45)
        _EdgeColor ("Edge Color", Color) = (0, 0.9, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            float _Start;
            float _End;
            float _Slant;
            float _EdgeWidth;
            float _GridX;
            float _GridY;
            float _GridThickness;
            fixed4 _DarkColor;
            fixed4 _GridColor;
            fixed4 _EdgeColor;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float x = i.uv.x + (i.uv.y - 0.5) * _Slant;
                float covered = step(_Start, x) * step(x, _End);

                float2 cell = frac(i.uv * float2(_GridX, _GridY));
                float grid = max(step(cell.x, _GridThickness), step(cell.y, _GridThickness));

                float edgeEnd = 1.0 - saturate(abs(x - _End) / _EdgeWidth);
                float edgeStart = 1.0 - saturate(abs(x - _Start) / _EdgeWidth);
                float edge = max(edgeEnd, edgeStart);

                fixed4 col = _DarkColor;
                col.rgb = lerp(col.rgb, _GridColor.rgb, grid * _GridColor.a);
                col.rgb = lerp(col.rgb, _EdgeColor.rgb, edge);
                col.a = saturate(covered + edge) * i.color.a;
                return col;
            }
            ENDCG
        }
    }
}