#pragma once
#include <memory>

#include "resource.h"

namespace digbuild::platform::render
{
	class TextureSampler : public Resource, public std::enable_shared_from_this<TextureSampler>
	{
	public:
		TextureSampler() = default;
		~TextureSampler() override = default;
		TextureSampler(const TextureSampler& other) = delete;
		TextureSampler(TextureSampler&& other) noexcept = delete;
		TextureSampler& operator=(const TextureSampler& other) = delete;
		TextureSampler& operator=(TextureSampler&& other) noexcept = delete;
	};
}
