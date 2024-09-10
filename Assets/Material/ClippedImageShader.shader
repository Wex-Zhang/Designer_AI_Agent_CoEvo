Shader "UI/ClippedVideo"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {} // 视频纹理
        _ClipMask ("Clip Mask", 2D) = "white" {} // 裁剪遮罩纹理
        _BorderThickness ("Border Thickness", Float) = 10.0 // 黑边厚度
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex; // 视频纹理
            sampler2D _ClipMask; // 裁剪遮罩
            float _BorderThickness; // 黑边厚度

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_ClipMask, i.uv);

                // 获取遮罩的亮度
                float maskValue = mask.r;

                // 如果遮罩值接近边界（0.5），显示黑色边界
                if (maskValue < 0.5 && maskValue > (0.5 - (_BorderThickness / 100.0)))
                {
                    return fixed4(0, 0, 0, 1); // 黑色边缘
                }

                // 裁剪掉黑色区域
                if (maskValue < 0.5)
                    discard;

                return col;
            }
            ENDCG
        }
    }
}
