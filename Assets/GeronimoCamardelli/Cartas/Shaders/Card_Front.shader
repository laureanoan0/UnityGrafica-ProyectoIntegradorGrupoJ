// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Card_Front"
{
	Properties
	{
		_HoloStrength("HoloStrength", Range( 0 , 1)) = 0
		_Disenodecarta("Diseno de carta", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_HoloArea("HoloArea", Range( 0 , 1)) = 0
		_IsGold("IsGold", Range( 0 , 1)) = 0
		_HoloMovement("_HoloMovement", Range( 0 , 1)) = 1
		_ColorSpeed("ColorSpeed", Range( 0 , 1)) = 0
		_BW_Map("BW_Map", 2D) = "white" {}
		_BW_Map_Full("BW_Map_Full", 2D) = "white" {}
		_HoloRamp("HoloRamp", 2D) = "white" {}
		_GoldRamp("GoldRamp", 2D) = "white" {}
		_FlowMap("FlowMap", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Disenodecarta;
		uniform float4 _Disenodecarta_ST;
		uniform sampler2D _GoldRamp;
		uniform sampler2D _FlowMap;
		uniform float4 _FlowMap_ST;
		uniform float _ColorSpeed;
		uniform float _HoloMovement;
		uniform sampler2D _HoloRamp;
		uniform float _IsGold;
		uniform sampler2D _BW_Map_Full;
		uniform float4 _BW_Map_Full_ST;
		uniform sampler2D _BW_Map;
		uniform float4 _BW_Map_ST;
		uniform float _HoloArea;
		uniform float _HoloStrength;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Disenodecarta = i.uv_texcoord * _Disenodecarta_ST.xy + _Disenodecarta_ST.zw;
			float4 tex2DNode1 = tex2D( _Disenodecarta, uv_Disenodecarta );
			float2 uv_FlowMap = i.uv_texcoord * _FlowMap_ST.xy + _FlowMap_ST.zw;
			float4 tex2DNode14_g5 = tex2D( _FlowMap, uv_FlowMap );
			float2 appendResult20_g5 = (float2(tex2DNode14_g5.r , tex2DNode14_g5.g));
			float lerpResult47 = lerp( ( _Time.y * _ColorSpeed ) , 0.0 , _HoloMovement);
			float TimeVar197_g5 = lerpResult47;
			float2 temp_cast_0 = (TimeVar197_g5).xx;
			float2 temp_output_18_0_g5 = ( appendResult20_g5 - temp_cast_0 );
			float4 tex2DNode72_g5 = tex2D( _GoldRamp, temp_output_18_0_g5 );
			float4 tex2DNode14_g6 = tex2D( _FlowMap, uv_FlowMap );
			float2 appendResult20_g6 = (float2(tex2DNode14_g6.r , tex2DNode14_g6.g));
			float TimeVar197_g6 = lerpResult47;
			float2 temp_cast_1 = (TimeVar197_g6).xx;
			float2 temp_output_18_0_g6 = ( appendResult20_g6 - temp_cast_1 );
			float4 tex2DNode72_g6 = tex2D( _HoloRamp, temp_output_18_0_g6 );
			float4 lerpResult34 = lerp( ( ( tex2DNode72_g5 * tex2DNode14_g5.a ) * tex2DNode1 ) , ( ( tex2DNode72_g6 * tex2DNode14_g6.a ) * tex2DNode1 ) , _IsGold);
			float2 uv_BW_Map_Full = i.uv_texcoord * _BW_Map_Full_ST.xy + _BW_Map_Full_ST.zw;
			float2 uv_BW_Map = i.uv_texcoord * _BW_Map_ST.xy + _BW_Map_ST.zw;
			float4 lerpResult18 = lerp( tex2D( _BW_Map_Full, uv_BW_Map_Full ) , tex2D( _BW_Map, uv_BW_Map ) , _HoloArea);
			float4 lerpResult13 = lerp( tex2DNode1 , lerpResult34 , ( lerpResult18 * _HoloStrength ));
			o.Albedo = lerpResult13.rgb;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
250;73;1268;495;2203.646;-812.5641;1.350527;True;False
Node;AmplifyShaderEditor.CommentaryNode;48;-3115.551,386.5179;Inherit;False;800.9602;398.7673;Velovidad de movieminto de los colores;5;47;43;2;46;3;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-3065.551,577.1622;Inherit;False;Property;_ColorSpeed;ColorSpeed;13;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;2;-3021.61,436.5179;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-2614.59,669.2852;Inherit;False;Property;_HoloMovement;_HoloMovement;12;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-2671.226,517.7234;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;41;-2267.621,-26.68995;Inherit;False;370;1015.782;seleccion de texturas y mapas;6;7;1;4;30;49;50;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;36;-1683.67,888.4187;Inherit;False;883.0896;592.0308;Seleccion entre holo completa y holo en bordes ;6;17;12;19;15;18;14;;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;47;-2527.621,511.1414;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;-1627.031,938.4187;Inherit;True;Property;_BW_Map_Full;BW_Map_Full;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;30;-2194.527,780.0246;Inherit;True;Property;_GoldRamp;GoldRamp;17;0;Create;True;0;0;0;False;0;False;None;4dafdc292ead91247b2ddc2f342991a2;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WireNode;49;-2247.872,468.6332;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;4;-2191.397,521.1094;Inherit;True;Property;_FlowMap;FlowMap;18;0;Create;True;0;0;0;False;0;False;None;cab88dafd64becd4aaa9df51b3e256b3;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WireNode;50;-2225.055,699.6836;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;39;-1697.54,280.2449;Inherit;False;893.8923;567.7964;Lerp selecciona entre un ramp dorado y un ramp multicolor;4;11;35;34;26;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-2183.169,254.033;Inherit;True;Property;_HoloRamp;HoloRamp;16;0;Create;True;0;0;0;False;0;False;None;35e19bedfc6e89545ab4d1771dcf8241;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;12;-1633.669,1158.211;Inherit;True;Property;_BW_Map;BW_Map;14;0;Create;True;0;0;0;False;0;False;-1;None;5bdfeb4f52cfc8b4bb28b162fbb50d3c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-2217.621,23.31005;Inherit;True;Property;_Disenodecarta;Diseno de carta;8;0;Create;True;0;0;0;False;0;False;1;None;2c0cace3496a5ed4898e5ea5dda31f71;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;-1601.572,1364.451;Inherit;False;Property;_HoloArea;HoloArea;10;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;-1269.573,1099.255;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-1095.03,525.929;Inherit;False;Property;_IsGold;IsGold;11;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-1120.301,1223.902;Inherit;False;Property;_HoloStrength;HoloStrength;7;0;Create;False;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;26;-1636.076,600.6125;Inherit;True;UI-Sprite Effect Layer;0;;5;789bf62641c5cfe4ab7126850acc22b8;18,74,1,204,1,191,1,225,0,242,0,237,0,249,0,186,0,177,0,182,0,229,0,92,1,98,0,234,0,126,0,129,1,130,0,31,0;18;192;COLOR;1,1,1,1;False;39;COLOR;1,1,1,1;False;37;SAMPLER2D;;False;218;FLOAT2;0,0;False;239;FLOAT2;0,0;False;181;FLOAT2;0,0;False;75;SAMPLER2D;;False;80;FLOAT;1;False;183;FLOAT2;0,0;False;188;SAMPLER2D;;False;33;SAMPLER2D;;False;248;FLOAT2;0,0;False;233;SAMPLER2D;;False;101;SAMPLER2D;;False;57;FLOAT4;0,0,0,0;False;40;FLOAT;0;False;231;FLOAT;1;False;30;FLOAT;1;False;2;COLOR;0;FLOAT2;172
Node;AmplifyShaderEditor.FunctionNode;11;-1634.534,370.3691;Inherit;True;UI-Sprite Effect Layer;0;;6;789bf62641c5cfe4ab7126850acc22b8;18,74,1,204,1,191,1,225,0,242,0,237,0,249,0,186,0,177,0,182,0,229,0,92,1,98,0,234,0,126,0,129,1,130,0,31,0;18;192;COLOR;1,1,1,1;False;39;COLOR;1,1,1,1;False;37;SAMPLER2D;;False;218;FLOAT2;0,0;False;239;FLOAT2;0,0;False;181;FLOAT2;0,0;False;75;SAMPLER2D;;False;80;FLOAT;1;False;183;FLOAT2;0,0;False;188;SAMPLER2D;;False;33;SAMPLER2D;;False;248;FLOAT2;0,0;False;233;SAMPLER2D;;False;101;SAMPLER2D;;False;57;FLOAT4;0,0,0,0;False;40;FLOAT;0;False;231;FLOAT;1;False;30;FLOAT;1;False;2;COLOR;0;FLOAT2;172
Node;AmplifyShaderEditor.CommentaryNode;42;-609.9247,-168.1811;Inherit;False;794.6183;521;Lerp final encargado de pintar solo en los lugares permitidos por BW_Map;2;0;13;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-962.5804,1104.059;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;34;-985.6475,402.5999;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;13;-559.9247,29.29943;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-70.3064,-118.1811;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Card_Front;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;9;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;43;0;2;0
WireConnection;43;1;46;0
WireConnection;47;0;43;0
WireConnection;47;2;3;0
WireConnection;49;0;47;0
WireConnection;50;0;47;0
WireConnection;18;0;17;0
WireConnection;18;1;12;0
WireConnection;18;2;19;0
WireConnection;26;39;1;0
WireConnection;26;37;30;0
WireConnection;26;33;4;0
WireConnection;26;40;50;0
WireConnection;11;39;1;0
WireConnection;11;37;7;0
WireConnection;11;33;4;0
WireConnection;11;40;49;0
WireConnection;14;0;18;0
WireConnection;14;1;15;0
WireConnection;34;0;26;0
WireConnection;34;1;11;0
WireConnection;34;2;35;0
WireConnection;13;0;1;0
WireConnection;13;1;34;0
WireConnection;13;2;14;0
WireConnection;0;0;13;0
WireConnection;0;10;1;4
ASEEND*/
//CHKSM=C3106090D359A0A05C046ADCEA6C2BF7EAF3BB25