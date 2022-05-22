Shader "HighlightPlus/Geometry/SeeThroughInner" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _SeeThrough ("See Through", Range(0,1)) = 0.8
    _SeeThroughTintColor ("See Through Tint Color", Color) = (1,0,0,0.8)
    _SeeThroughNoise("Noise", Float) = 1
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _CutOff("CutOff", Float ) = 0.5
    _SeeThroughBorderWidth ("Outline Offset", Float) = 0.01
    _SeeThroughBorderConstantWidth ("Constant Width", Float) = 1
    _SeeThroughStencilRef ("Stencil Ref", Int) = 2
    _SeeThroughStencilComp ("Stencil Comp", Int) = 5
}
    SubShader
    {
        UsePass "Standard/DEFERRED"
   }
}