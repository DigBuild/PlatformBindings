#pragma once
#include "../../render/texture.h"

namespace digbuild::platform::desktop::vulkan
{
	class Texture : public render::Texture
	{
	public:
		virtual vk::ImageView& get() = 0;
	};

	// class StaticTexture : public Texture
	// {
	// 	
	// };
}
