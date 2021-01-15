#pragma once
#include <memory>

namespace digbuild::platform::render
{
	class VertexBuffer : public std::enable_shared_from_this<VertexBuffer>
	{
	public:
		VertexBuffer() = default;
		virtual ~VertexBuffer() = default;
		VertexBuffer(const VertexBuffer& other) = delete;
		VertexBuffer(VertexBuffer&& other) noexcept = delete;
		VertexBuffer& operator=(const VertexBuffer& other) = delete;
		VertexBuffer& operator=(VertexBuffer&& other) noexcept = delete;

		virtual void update(const std::vector<char>& data) = 0;
	};
}
