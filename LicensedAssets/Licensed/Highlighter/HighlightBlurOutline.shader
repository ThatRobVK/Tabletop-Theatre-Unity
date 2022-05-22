Shader "HighlightPlus/Geometry/BlurOutline" {
Properties {
    _MainTex ("Texture", Any) = "white" {}
    _Color ("Color", Color) = (1,1,0) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _BlurScale("Blur Scale", Float) = 2.0
    _StereoRendering("Stereo Rendering Correction", Float) = 1
}
    SubShader
    {
        UsePass "Standard/DEFERRED"
	}
}