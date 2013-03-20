Shader "CW/Transparent" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
		
	Tags {"Queue"="Transparent"}
	
	Pass {
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha 
		ColorMask RGB
		ColorMaterial AmbientAndDiffuse
	
		//Lighting On
		SetTexture [_MainTex] {
			Combine texture * primary DOUBLE, texture * primary 
		} 
	}		
	
	/*
	Pass {
		ZWrite On
		ColorMask 0
	}	

	Pass {
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		ColorMask RGB
		ColorMaterial AmbientAndDiffuse
	
		//Lighting On
		SetTexture [_MainTex] {
			Combine texture * primary DOUBLE, texture * primary 
		} 
	}	
	*/
}
}