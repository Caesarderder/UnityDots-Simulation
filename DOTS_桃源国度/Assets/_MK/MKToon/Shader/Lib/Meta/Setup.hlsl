//////////////////////////////////////////////////////
// MK Toon Meta Setup				       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_META_SETUP
	#define MK_TOON_META_SETUP

	#ifndef MK_META_PASS
		#define MK_META_PASS
	#endif
	
	#include "../Core.hlsl"

	//Hightmap is only needed if a UV is required
	#if !defined(MK_TEXCLR) && !defined(MK_DISSOLVE)
		#ifdef MK_PARALLAX
			#undef MK_PARALLAX
		#endif
	#endif

	#include "ProgramMeta.hlsl"
#endif