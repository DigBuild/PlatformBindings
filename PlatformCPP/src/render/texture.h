#pragma once
#include <memory>

#include "resource.h"

namespace digbuild::platform::render
{
	enum class TextureFormat : uint8_t
	{
		R8G8B8A8_SRGB,
		B8G8R8A8_SRGB,
		D32SFLOAT_S8UINT = 0xFF
	};

	class Texture : public Resource, public std::enable_shared_from_this<Texture>
	{
	public:
		[[nodiscard]] virtual uint32_t getWidth() = 0;
		[[nodiscard]] virtual uint32_t getHeight() = 0;
	};
}
