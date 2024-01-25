//////////////////////////////////////////////////////
// MK Toon Renderers Setup					       	//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_RENDERERS_SETUP
	#define MK_TOON_RENDERERS_SETUP
	#if SHADER_TARGET >= 45
		#pragma exclude_renderers gles gles3 glcore d3d11_9x wiiu n3ds switch
	#elif SHADER_TARGET >= 35
		#pragma exclude_renderers gles d3d11_9x ps4 ps5 xboxone
	#else
		#pragma exclude_renderers gles3 d3d11 ps4 ps5 xboxone wiiu n3ds switch
	#endif
#endif