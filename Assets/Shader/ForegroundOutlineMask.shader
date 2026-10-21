Shader "UniTopia/ForegroundOutlineMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Stencil
        {
            Ref 64
            WriteMask 64
            Comp Always
            Pass Replace
        }

        Pass
        {
            CGPROGRAM

            #pragma vertex SpriteVert
            #pragma fragment ForegroundFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnitySprites.cginc"

            fixed4 ForegroundFragment(v2f input) : SV_Target
            {
                fixed4 color =
                    SampleSpriteTexture(input.texcoord) * input.color;

                // Transparent parts must not mark the screen.
                clip(color.a - 0.1);

                color.rgb *= color.a;
                return color;
            }

            ENDCG
        }
    }
}