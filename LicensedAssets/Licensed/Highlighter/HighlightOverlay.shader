Shader "HighlightPlus/Geometry/Overlay" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
    _OverlayBackColor ("Overlay Back Color", Color) = (1,1,1,1)
    _OverlayData("Overlay Data", Vector) = (1,0.5,1)
    _CutOff("CutOff", Float ) = 0.5
    _Cull ("Cull Mode", Int) = 2
}
    SubShader
    {
        UsePass "Standard/DEFERRED"
   }
}