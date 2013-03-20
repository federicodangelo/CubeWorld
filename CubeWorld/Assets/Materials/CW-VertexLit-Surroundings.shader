Shader "CW/Surroundings-Normal" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Color ("Main Color", COLOR) = (1,1,1,1)
}

SubShader {
	//Tags { "RenderType"="Opaque" }
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}


	Pass {
		Tags { "LightMode" = "Vertex" }

		//ColorMaterial AmbientAndDiffuse
		//Lighting On
		

		Alphatest Greater 0
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha 
		ColorMask RGB


		Color [_Color]
		
		SetTexture [_MainTex] {
			Combine texture * primary DOUBLE, texture * primary 
		} 
	}	
}
}
