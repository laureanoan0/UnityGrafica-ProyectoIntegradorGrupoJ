// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Card_Front"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_HoloStrength("HoloStrength", Range( 0 , 1)) = 0
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_HoloArea("HoloArea", Range( 0 , 1)) = 0
		_ColorTime("ColorTime", Float) = 1
		_BW_Map("BW_Map", 2D) = "white" {}
		_BW_Map_Full("BW_Map_Full", 2D) = "white" {}
		_Ramp("Ramp", 2D) = "white" {}
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

		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform sampler2D _Ramp;
		uniform sampler2D _FlowMap;
		uniform float4 _FlowMap_ST;
		uniform float _ColorTime;
		uniform sampler2D _BW_Map_Full;
		uniform float4 _BW_Map_Full_ST;
		uniform sampler2D _BW_Map;
		uniform float4 _BW_Map_ST;
		uniform float _HoloArea;
		uniform float _HoloStrength;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float4 tex2DNode1 = tex2D( _TextureSample0, uv_TextureSample0 );
			float2 uv_FlowMap = i.uv_texcoord * _FlowMap_ST.xy + _FlowMap_ST.zw;
			float4 tex2DNode14_g4 = tex2D( _FlowMap, uv_FlowMap );
			float2 appendResult20_g4 = (float2(tex2DNode14_g4.r , tex2DNode14_g4.g));
			float TimeVar197_g4 = ( _Time.y * _ColorTime );
			float2 temp_cast_0 = (TimeVar197_g4).xx;
			float2 temp_output_18_0_g4 = ( appendResult20_g4 - temp_cast_0 );
			float4 tex2DNode72_g4 = tex2D( _Ramp, temp_output_18_0_g4 );
			float2 uv_BW_Map_Full = i.uv_texcoord * _BW_Map_Full_ST.xy + _BW_Map_Full_ST.zw;
			float2 uv_BW_Map = i.uv_texcoord * _BW_Map_ST.xy + _BW_Map_ST.zw;
			float4 lerpResult18 = lerp( tex2D( _BW_Map_Full, uv_BW_Map_Full ) , tex2D( _BW_Map, uv_BW_Map ) , _HoloArea);
			float4 lerpResult13 = lerp( tex2DNode1 , ( ( tex2DNode72_g4 * tex2DNode14_g4.a ) * tex2DNode1 ) , ( lerpResult18 * _HoloStrength ));
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
252;73;1266;521;2441.712;551.5574;2.164523;True;False
Node;AmplifyShaderEditor.SimpleTimeNode;2;-1415.392,657.5072;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-1398.971,761.1373;Inherit;False;Property;_ColorTime;ColorTime;11;0;Create;True;0;0;0;False;0;False;1;0.83;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;-1438.144,-328.7762;Inherit;True;Property;_BW_Map;BW_Map;12;0;Create;True;0;0;0;False;0;False;-1;None;0246686bfc07a284bbf538f4ebbcb026;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;17;-1437.959,-527.603;Inherit;True;Property;_BW_Map_Full;BW_Map_Full;13;0;Create;True;0;0;0;False;0;False;-1;None;0246686bfc07a284bbf538f4ebbcb026;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;-1406.047,-124.7307;Inherit;False;Property;_HoloArea;HoloArea;10;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-1234.512,630.5255;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-1536.091,232.2231;Inherit;True;Property;_Ramp;Ramp;14;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;4;-1591.509,453.4766;Inherit;True;Property;_FlowMap;FlowMap;15;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;1;-1630.498,22.33369;Inherit;True;Property;_TextureSample0;Texture Sample 0;9;0;Create;True;0;0;0;False;0;False;1;None;e91417445753cc44e84f4b722503f872;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;18;-1074.047,-387.7307;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-924.7746,-263.0851;Inherit;False;Property;_HoloStrength;HoloStrength;8;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;11;-829.5659,351.4026;Inherit;True;UI-Sprite Effect Layer;0;;4;789bf62641c5cfe4ab7126850acc22b8;18,74,1,204,1,191,1,225,0,242,0,237,0,249,0,186,0,177,0,182,0,229,0,92,1,98,0,234,0,126,0,129,1,130,0,31,0;18;192;COLOR;1,1,1,1;False;39;COLOR;1,1,1,1;False;37;SAMPLER2D;;False;218;FLOAT2;0,0;False;239;FLOAT2;0,0;False;181;FLOAT2;0,0;False;75;SAMPLER2D;;False;80;FLOAT;1;False;183;FLOAT2;0,0;False;188;SAMPLER2D;;False;33;SAMPLER2D;;False;248;FLOAT2;0,0;False;233;SAMPLER2D;;False;101;SAMPLER2D;;False;57;FLOAT4;0,0,0,0;False;40;FLOAT;0;False;231;FLOAT;1;False;30;FLOAT;1;False;2;COLOR;0;FLOAT2;172
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-767.0539,-382.9275;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;13;-429.2118,39.50539;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-70.3064,-119.2559;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Card_Front;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;7;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;2;0
WireConnection;5;1;3;0
WireConnection;18;0;17;0
WireConnection;18;1;12;0
WireConnection;18;2;19;0
WireConnection;11;39;1;0
WireConnection;11;37;7;0
WireConnection;11;33;4;0
WireConnection;11;40;5;0
WireConnection;14;0;18;0
WireConnection;14;1;15;0
WireConnection;13;0;1;0
WireConnection;13;1;11;0
WireConnection;13;2;14;0
WireConnection;0;0;13;0
WireConnection;0;10;1;4
ASEEND*/
//CHKSM=79EF779BA1C7BD8AEFC80EBCF6197A5E9F5505B7