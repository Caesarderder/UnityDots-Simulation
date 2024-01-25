//////////////////////////////////////////////////////
// MK Toon ShadowCaster Setup			       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_SHADOWCASTER_SETUP
	#define MK_TOON_SHADOWCASTER_SETUP

	#ifndef MK_SHADOWCASTER_PASS
		#define MK_SHADOWCASTER_PASS
	#endif

	#include "../Core.hlsl"

	#if defined(MK_SURFACE_TYPE_TRANSPARENT) && SHADER_TARGET > 30
		#ifndef MK_TOON_DITHER_MASK
			#define MK_TOON_DITHER_MASK
		#endif
	#endif

	//Hightmap is only needed if a UV is required
	#if !defined(MK_TEXCLR) && !defined(MK_DISSOLVE)
		#ifdef MK_PARALLAX
			#undef MK_PARALLAX
		#endif
	#endif

	#include "ProgramShadowCaster.hlsl"
#endif