//////////////////////////////////////////////////////
// MK Toon Depth Normals Setup			       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_DEPTH_NORMALS_SETUP
	#define MK_TOON_DEPTH_NORMALS_SETUP

	#ifndef MK_DEPTH_NORMALS_PASS
		#define MK_DEPTH_NORMALS_PASS
	#endif

	#include "../Core.hlsl"

	//Hightmap is only needed if a UV is required
	#if !defined(MK_TEXCLR) && !defined(MK_DISSOLVE)
		#ifdef MK_PARALLAX
			#undef MK_PARALLAX
		#endif
	#endif

	#include "ProgramDepthNormals.hlsl"
#endif