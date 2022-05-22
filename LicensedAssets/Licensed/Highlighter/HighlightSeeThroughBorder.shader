Shader "HighlightPlus/Geometry/SeeThroughBorder" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _SeeThroughBorderColor ("Outline Color", Color) = (0,0,0,1)
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _CutOff("CutOff", Float ) = 0.5
    _SeeThroughStencilRef ("Stencil Ref", Int) = 2
    _SeeThroughStencilComp ("Stencil Comp", Int) = 5
}
    SubShader
    {
        UsePass "Standard/DEFERRED"
   }
}