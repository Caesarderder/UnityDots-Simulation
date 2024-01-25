//////////////////////////////////////////////////////
// MK Toon Global Shader Features					//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_GLOBAL_SHADER_FEATURES
	//Enable vertex colors be combined with the albedo map
	//%%MK_COMBINE_VERTEX_COLOR_WITH_ALBEDO_MAP
	/*!!
	#ifndef MK_COMBINE_VERTEX_COLOR_WITH_ALBEDO_MAP
		#define MK_COMBINE_VERTEX_COLOR_WITH_ALBEDO_MAP
	#endif
	$$*/
	
	//Enable outline distance based fading
	//Also set compile directive on project window: MK_TOON_OUTLINE_FADING
	// Customize the outline fading style (linear by default)
	// Outline Fade has to be enabled
	//%%MK_OUTLINE_FADING_LINEAR
	/*!!
	#ifndef MK_OUTLINE_FADING_LINEAR
		#define MK_OUTLINE_FADING_LINEAR
	#endif
	$$*/
	
	//%%MK_OUTLINE_FADING_EXPONENTIAL
	/*!!
	#ifndef MK_OUTLINE_FADING_EXPONENTIAL
		#define MK_OUTLINE_FADING_EXPONENTIAL
	#endif
	$$*/

	//%%MK_OUTLINE_FADING_INVERSE_EXPONENTIAL
	/*!!
	#ifndef MK_OUTLINE_FADING_INVERSE_EXPONENTIAL
		#define MK_OUTLINE_FADING_INVERSE_EXPONENTIAL
	#endif
	$$*/

	//Enable Point Filtering instead of Bilinear
	//This can't be customized by the user in any other way, since sampler objects has to be are hardcoded
	//%%MK_POINT_FILTERING
	/*!!
	#ifndef MK_POINT_FILTERING
		#define MK_POINT_FILTERING
	#endif
	$$*/

	//Enable screen spaced dissolve instead of tangent spaced
	//%%MK_DISSOLVE_PROJECTION_SCREEN_SPACE
	/*!!
	#ifndef MK_DISSOLVE_PROJECTION_SCREEN_SPACE
		#define MK_DISSOLVE_PROJECTION_SCREEN_SPACE
	#endif
	$$*/

	//Enable Local Antialiasing
	//%%MK_LOCAL_ANTIALIASING
	//!!
	#ifndef MK_LOCAL_ANTIALIASING
		#define MK_LOCAL_ANTIALIASING
	#endif
	//$$

	//Force Linear Light Attenuation on URP - experimental
	//%%MK_LINEAR_lIGHT_DISTANCE_ATTENUATION
	/*!!
	#ifndef MK_LINEAR_lIGHT_DISTANCE_ATTENUATION
		#define MK_LINEAR_lIGHT_DISTANCE_ATTENUATION
	#endif
	$$*/

	//Enable stylized shadows
	//%%MK_STYLIZE_SYSTEM_SHADOWS
	/*!!
	#ifndef MK_STYLIZE_SYSTEM_SHADOWS
		#define MK_STYLIZE_SYSTEM_SHADOWS
	#endif
	$$*/
	
	//Enable Legacy Screen Spaced Scaling
	//%%MK_LEGACY_SCREEN_SCALING
	/*!!
	#ifndef MK_LEGACY_SCREEN_SCALING
		#define MK_LEGACY_SCREEN_SCALING
	#endif
	$$*/

	//Enable multi pass scaling for screen space
	//%%MK_MULTI_PASS_STEREO_SCALING
	/*!!
	#ifndef MK_MULTI_PASS_STEREO_SCALING
		#define MK_MULTI_PASS_STEREO_SCALING
	#endif
	$$*/

	//Enable legacy noise
	//%%MK_LEGACY_NOISE
	/*!!
	#ifndef MK_LEGACY_NOISE
		#define MK_LEGACY_NOISE
	#endif
	$$*/

	//Enable legacy noise
	//%%MK_REGULAR_SCREEN_SPACE
	/*!!
	#ifndef MK_REGULAR_SCREEN_SPACE
		#define MK_REGULAR_SCREEN_SPACE
	#endif
	$$*/
#endif
