#pragma once
#include <cstdint>

namespace digbuild::platform::util
{
	union Vector2
	{
		uint8_t data[2 * 4];
	};
	union Vector3
	{
		uint8_t data[3 * 4];
	};
	union Vector4
	{
		uint8_t data[4 * 4];
	};
	
	struct Extents2D
	{
		const uint32_t x, y;
		const uint32_t width, height;
	};
}
