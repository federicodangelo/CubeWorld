Shader "CW/Translucid" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Geometry+1" "IgnoreProjector"="True" "RenderType"="Transparent"}
	
	Alphatest Greater 0
	ZWrite On
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask RGB
	ColorMaterial AmbientAndDiffuse
		
	Pass {
		Tags { "LightMode" = "Always" }
		//Lighting On
		SetTexture [_MainTex] {
			Combine texture * primary DOUBLE, texture * primary 
		} 
	}	
}
}