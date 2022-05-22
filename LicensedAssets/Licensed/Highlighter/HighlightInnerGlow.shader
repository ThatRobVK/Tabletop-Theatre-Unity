Shader "HighlightPlus/Geometry/InnerGlow" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
    _InnerGlowWidth ("Width", Float) = 1.0
    _CutOff("CutOff", Float ) = 0.5
    _Cull ("Cull Mode", Int) = 2
    _InnerGlowZTest ("ZTest", Int) = 4
}
    SubShader
    {
        UsePass "Standard/DEFERRED"
   }
}