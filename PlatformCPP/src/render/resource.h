#pragma once

namespace digbuild::platform::render
{
	class Resource
	{
	public:
		Resource() = default;
		virtual ~Resource() = default;
		Resource(const Resource& other) = delete;
		Resource(Resource&& other) noexcept = delete;
		Resource& operator=(const Resource& other) = delete;
		Resource& operator=(Resource&& other) noexcept = delete;
	};
}
