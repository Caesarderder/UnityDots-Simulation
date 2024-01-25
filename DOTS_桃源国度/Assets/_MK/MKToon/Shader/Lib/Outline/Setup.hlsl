//////////////////////////////////////////////////////
// MK Toon Outline Setup			       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_OUTLINE_SETUP
	#define MK_TOON_OUTLINE_SETUP

	#ifndef MK_OUTLINE_PASS
		#define MK_OUTLINE_PASS
	#endif

	#include "../Core.hlsl"

	//Hightmap is only needed if a UV is required
	#if !defined(MK_TEXCLR) && !defined(MK_DISSOLVE)
		#ifdef MK_PARALLAX
			#undef MK_PARALLAX
		#endif
	#endif

	#include "ProgramOutline.hlsl"
#endif