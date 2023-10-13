// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Habitat/Object Highlight" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _FadeOutDistanceStart ("Fade Out Distance Start", float) = 2.5
        _FadeOutDistanceEnd ("Fade Out Distance End", float) = 5
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4 //"LEqual"
    }

    SubShader {

        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Lighting Off
        Cull Off
        ZTest [_ZTest]
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float distanceFromCamera : float;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform fixed4 _Color;
            uniform float _FadeOutDistanceStart;
            uniform float _FadeOutDistanceEnd;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;

                // Fade-out the highlight based on the distance from the camera
                float distanceFromCamera = length(ObjSpaceViewDir(v.vertex));
                o.color.a *= 1.0f - clamp((distanceFromCamera - _FadeOutDistanceStart)/(_FadeOutDistanceEnd - _FadeOutDistanceStart), 0.0f, 1.0f);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color;
                return col;
            }
            ENDCG
        }
    }
}
