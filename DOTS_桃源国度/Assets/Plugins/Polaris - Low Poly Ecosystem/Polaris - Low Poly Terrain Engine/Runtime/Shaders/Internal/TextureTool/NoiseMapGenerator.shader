Shader "Hidden/Griffin/NoiseMapGenerator"
{
    Properties
    {
        _Origin ("Origin", Vector) = (0,0,0,0)
		_Frequency ("Frequency", Float) = 1
		_Lacunarity ("Lacunarity", Float) = 2
		_Persistence ("Persistence", Float) = 0.5
		_Octaves ("Octaves", Int) = 1
		_Seed ("Seed", float) = 12345
    }

	CGINCLUDE
    #include "UnityCG.cginc"
#include "TextureToolCommon.cginc"

	struct appdata
    {
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
    };

	float4 _Origin;
	float _Frequency;
	float _Lacunarity;
	float _Persistence;
	int _Octaves;
	float _Seed;

    struct v2f
    {
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
    };

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
    }

	float GetRandomNumber(float seed)
	{
		return seed*23.456*(1+ceil(seed)*12.345);
	}

	float4 fragPerlin (v2f i) : SV_Target
    {
		float2 refSize = float2(1,1);
		float noiseValue = 0; //-1 to 1
		float frequency = _Frequency;
		float amplitude = 1;
		float2 noisePos;
		float2 uv;
		float sampledNoise;
		float rand = GetRandomNumber(_Seed);
		float2 offset = float2(rand, rand);

		for (int octave=0; octave<4; ++octave)
		{
			frequency = frequency*pow(_Lacunarity, octave);
			amplitude = amplitude*pow(_Persistence, octave);
			noisePos = _Origin.xy + i.uv*frequency + offset;
			uv = float2(noisePos.x/refSize.x, noisePos.y/refSize.y);
			sampledNoise = GradientNoise(uv)*amplitude*(octave<_Octaves);
			noiseValue += sampledNoise;
		}

		noiseValue = (noiseValue+1)*0.5; //remap to 0-1
		return float4(noiseValue, noiseValue, noiseValue, noiseValue);
	}

	float4 fragBillow (v2f i) : SV_Target
	{
		float2 refSize = float2(1,1);
		float noiseValue = 0; //-1 to 1
		float frequency = _Frequency;
		float amplitude = 1;
		float2 noisePos;
		float2 uv;
		float sampledNoise;
		float rand = GetRandomNumber(_Seed);
		float2 offset = float2(rand, rand);

		for (int octave=0; octave<4; ++octave)
		{
			frequency = frequency*pow(_Lacunarity, octave);
			amplitude = amplitude*pow(_Persistence, octave);
			noisePos = _Origin.xy + i.uv*frequency + offset;
			uv = float2(noisePos.x/refSize.x, noisePos.y/refSize.y);
			sampledNoise = GradientNoise(uv)*amplitude*(octave<_Octaves);
			noiseValue += abs(sampledNoise);
		}

		noiseValue = abs(noiseValue);
		return float4(noiseValue, noiseValue, noiseValue, noiseValue);
	}

	float4 fragRidged (v2f i) : SV_Target
	{
		float2 refSize = float2(1,1);
		float noiseValue = 0; //-1 to 1
		float frequency = _Frequency;
		float amplitude = 1;
		float2 noisePos;
		float2 uv;
		float sampledNoise;
		float rand = GetRandomNumber(_Seed);
		float2 offset = float2(rand, rand);

		for (int octave=0; octave<4; ++octave)
		{
			frequency = frequency*pow(_Lacunarity, octave);
			amplitude = amplitude*pow(_Persistence, octave);
			noisePos = _Origin.xy + i.uv*frequency + offset;
			uv = float2(noisePos.x/refSize.x, noisePos.y/refSize.y);
			sampledNoise = GradientNoise(uv)*amplitude*(octave<_Octaves);
			noiseValue += abs(sampledNoise);
		}

		noiseValue = 1-abs(noiseValue);
		return float4(noiseValue, noiseValue, noiseValue, noiseValue);
	}

	float4 fragVoronoi (v2f i) : SV_Target
	{
		float2 refSize = float2(1,1);
		float noiseValue = 0; //0 to 1
		float frequency = _Frequency;
		float amplitude = 1;
		float2 noisePos;
		float2 uv;
		float sampledNoise;
		
		for (int octave=0; octave<4; ++octave)
		{
			frequency = frequency*pow(_Lacunarity, octave);
			amplitude = amplitude*pow(_Persistence, octave);
			uv = _Origin+i.uv;
			sampledNoise = Voronoi(uv, _Seed, frequency)*amplitude*(octave<_Octaves);
			noiseValue += sampledNoise;
		}

		return float4(noiseValue, noiseValue, noiseValue, noiseValue);
	}

	float4 fragValue (v2f i) : SV_Target
    {
		float2 refSize = float2(1,1);
		float noiseValue = 0; //-1 to 1
		float frequency = _Frequency;
		float amplitude = 1;
		float2 noisePos;
		float2 uv;
		float sampledNoise;
		float rand = GetRandomNumber(_Seed);
		float2 offset = float2(rand, rand);

		for (int octave=0; octave<4; ++octave)
		{
			frequency = frequency*pow(_Lacunarity, octave);
			amplitude = amplitude*pow(_Persistence, octave);
			noisePos = _Origin.xy + i.uv*frequency + offset;
			uv = float2(noisePos.x/refSize.x, noisePos.y/refSize.y);
			sampledNoise = ValueNoise(uv)*amplitude*(octave<_Octaves);
			noiseValue += sampledNoise;
		}

		noiseValue = (noiseValue+1)*0.5; //remap to 0-1
		return float4(noiseValue, noiseValue, noiseValue, noiseValue);
	}

	ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
		
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragPerlin
            ENDCG
        }
		
		Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBillow
            ENDCG
        }

		Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragRidged
            ENDCG
        }

		Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragVoronoi
            ENDCG
        }

		Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragValue
            ENDCG
        }
    }
}
