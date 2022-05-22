Shader "HighlightPlus/Geometry/Mask" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _CutOff("CutOff", Float ) = 0.5
    _Cull ("Cull Mode", Int) = 2
	_ZTest ("ZTest", Int) = 4
}
    SubShader
    {
        UsePass "Standard/DEFERRED"
   }
}