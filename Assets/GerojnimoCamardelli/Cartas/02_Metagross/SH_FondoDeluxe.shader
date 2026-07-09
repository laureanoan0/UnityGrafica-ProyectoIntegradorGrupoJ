// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SH_Fondo"
{
	Properties
	{
		_Disenodecarta1("Diseno de carta", 2D) = "white" {}
		_ColorTime1("ColorTime", Float) = 1
		_HoloRamp1("HoloRamp", 2D) = "white" {}
		_FlowMap1("FlowMap", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		ZWrite On
		ZTest Always
		Stencil
		{
			Ref 1
			Comp Equal
			Pass Keep
			Fail Keep
			ZFail Keep
		}
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _HoloRamp1;
		uniform sampler2D _FlowMap1;
		uniform float4 _FlowMap1_ST;
		uniform float _ColorTime1;
		uniform sampler2D _Disenodecarta1;
		uniform float4 _Disenodecarta1_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_FlowMap1 = i.uv_texcoord * _FlowMap1_ST.xy + _FlowMap1_ST.zw;
			float4 tex2DNode14_g4 = tex2D( _FlowMap1, uv_FlowMap1 );
			float2 appendResult20_g4 = (float2(tex2DNode14_g4.r , tex2DNode14_g4.g));
			float TimeVar197_g4 = ( _Time.y * _ColorTime1 );
			float2 temp_cast_0 = (TimeVar197_g4).xx;
			float2 temp_output_18_0_g4 = ( appendResult20_g4 - temp_cast_0 );
			float4 tex2DNode72_g4 = tex2D( _HoloRamp1, temp_output_18_0_g4 );
			float2 uv_Disenodecarta1 = i.uv_texcoord * _Disenodecarta1_ST.xy + _Disenodecarta1_ST.zw;
			o.Albedo = ( ( tex2DNode72_g4 * tex2DNode14_g4.a ) * tex2D( _Disenodecarta1, uv_Disenodecarta1 ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
250;73;1268;638;2828.599;359.6225;2.846505;True;False
Node;AmplifyShaderEditor.CommentaryNode;3;-1869.624,676.5426;Inherit;False;490.7578;383.6132;Velovidad de movieminto de los colores;4;10;8;5;4;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleTimeNode;4;-1819.624,828.5562;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-1803.202,932.1862;Inherit;False;Property;_ColorTime1;ColorTime;8;0;Create;True;0;0;0;False;0;False;1;0.83;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;6;-1826.625,-80.98792;Inherit;False;370;1015.782;seleccion de texturas y mapas;3;18;13;9;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-1638.743,801.5743;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;17;-1256.544,225.9469;Inherit;False;893.8923;567.7964;Lerp selecciona entre un ramp dorado y un ramp multicolor;2;19;0;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;9;-1776.625,-30.98795;Inherit;True;Property;_Disenodecarta1;Diseno de carta;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;10;-1460.866,726.5425;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;13;-1717.92,441.9723;Inherit;True;Property;_FlowMap1;FlowMap;10;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;18;-1742.173,199.735;Inherit;True;Property;_HoloRamp1;HoloRamp;9;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FunctionNode;19;-1195.449,275.9469;Inherit;True;UI-Sprite Effect Layer;0;;4;789bf62641c5cfe4ab7126850acc22b8;18,74,1,204,1,191,1,225,0,242,0,237,0,249,0,186,0,177,0,182,0,229,0,92,1,98,0,234,0,126,0,129,1,130,0,31,0;18;192;COLOR;1,1,1,1;False;39;COLOR;1,1,1,1;False;37;SAMPLER2D;;False;218;FLOAT2;0,0;False;239;FLOAT2;0,0;False;181;FLOAT2;0,0;False;75;SAMPLER2D;;False;80;FLOAT;1;False;183;FLOAT2;0,0;False;188;SAMPLER2D;;False;33;SAMPLER2D;;False;248;FLOAT2;0,0;False;233;SAMPLER2D;;False;101;SAMPLER2D;;False;57;FLOAT4;0,0,0,0;False;40;FLOAT;0;False;231;FLOAT;1;False;30;FLOAT;1;False;2;COLOR;0;FLOAT2;172
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-670.0212,315.3527;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SH_Fondo;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;1;False;-1;7;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;True;1;False;-1;255;False;-1;255;False;-1;5;False;-1;1;False;-1;1;False;-1;1;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;7;-1242.674,834.121;Inherit;False;883.0896;592.0308;Seleccion entre holo completa y holo en bordes ;0;;1,1,1,1;0;0
WireConnection;8;0;4;0
WireConnection;8;1;5;0
WireConnection;10;0;8;0
WireConnection;19;39;9;0
WireConnection;19;37;18;0
WireConnection;19;33;13;0
WireConnection;19;40;10;0
WireConnection;0;0;19;0
ASEEND*/
//CHKSM=6C27420D227A5C4695A4A603C0944CC65AA88312