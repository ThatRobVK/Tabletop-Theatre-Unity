Shader "HighlightPlus/Geometry/Target" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
    _ZTest ("ZTest", Int) = 0
}
    SubShader
    {
        UsePass "Standard/DEFERRED"
   }
}