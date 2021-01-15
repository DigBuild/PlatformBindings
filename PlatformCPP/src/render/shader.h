#pragma once
#include <memory>

namespace digbuild::platform::render
{
	enum class ShaderType
	{
		VERTEX,
		FRAGMENT
	};

	class Shader : public std::enable_shared_from_this<Shader>
	{
	public:
		Shader() = default;
		virtual ~Shader() = default;
		Shader(const Shader& other) = delete;
		Shader(Shader&& other) noexcept = delete;
		Shader& operator=(const Shader& other) = delete;
		Shader& operator=(Shader&& other) noexcept = delete;
	};
}
