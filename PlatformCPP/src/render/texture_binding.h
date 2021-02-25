#pragma once
#include <memory>

#include "resource.h"
#include "texture.h"
#include "texture_sampler.h"

namespace digbuild::platform::render
{
	class TextureBinding : public Resource, public std::enable_shared_from_this<TextureBinding>
	{
	public:
		TextureBinding() = default;
		~TextureBinding() override = default;
		TextureBinding(const TextureBinding& other) = delete;
		TextureBinding(TextureBinding&& other) noexcept = delete;
		TextureBinding& operator=(const TextureBinding& other) = delete;
		TextureBinding& operator=(TextureBinding&& other) noexcept = delete;

		virtual void update(
			std::shared_ptr<TextureSampler> sampler,
			std::shared_ptr<Texture> texture
		) = 0;
	};
}
