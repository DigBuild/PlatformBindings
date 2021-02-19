#pragma once
#include "resource.h"
#include <memory>

namespace digbuild::platform::render
{
	class Shader : public Resource, public std::enable_shared_from_this<Shader>
	{
	public:
		Shader() = default;
		~Shader() override = default;
		Shader(const Shader& other) = delete;
		Shader(Shader&& other) noexcept = delete;
		Shader& operator=(const Shader& other) = delete;
		Shader& operator=(Shader&& other) noexcept = delete;
	};
}
