//////////////////////////////////////////////////////
// MK Toon Dots Instancing Setup					//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_DOTS_INSTANCING_SETUP
	#define MK_TOON_DOTS_INSTANCING_SETUP

	#if UNITY_VERSION >= 202230
		#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
	#elif UNITY_VERSION >= 202030
		#pragma multi_compile __ DOTS_INSTANCING_ON
		#pragma target 3.5 DOTS_INSTANCING_ON
		#pragma target 4.5 DOTS_INSTANCING_ON
	#endif
#endif