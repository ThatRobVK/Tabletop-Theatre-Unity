// This shader has had its guts removed for open sourcing and should be replaced with the correct shader for release
Shader "Custom/FCP_Gradient" {
	Properties{
		[PerRendererData][Enum(Horizontal,0,Vertical,1,Double,2,HueHorizontal,3,HueVertical,4)] _Mode("Color mode", Int) = 0
		[PerRendererData]_Color1("Color 1", Color) = (1,1,1,1)
		[PerRendererData]_Color2("Color 2", Color) = (1,1,1,1)
		[PerRendererData][Enum(HS, 0, HV, 1, SH, 2, SV, 3, VH, 4, VS, 5)] _DoubleMode("Double mode", Int) = 0
		[PerRendererData]_HSV("Complementing HSV values", Vector) = (0,0,0,1.0)
		[PerRendererData]_HSV_MIN("Min Range value for HSV", Vector) = (0.0,0.0,0.0,0.0)
		[PerRendererData]_HSV_MAX("Max Range value for HSV", Vector) = (1.0,1.0,1.0,1.0)
	}

	SubShader{
        UsePass "Standard/DEFERRED"
	}
}