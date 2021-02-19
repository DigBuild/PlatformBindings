#pragma once
#include <memory>
#include <vector>

#include "resource.h"

namespace digbuild::platform::render
{
	class VertexBuffer : public Resource, public std::enable_shared_from_this<VertexBuffer>
	{
	public:
		VertexBuffer() = default;
		~VertexBuffer() override = default;
		VertexBuffer(const VertexBuffer& other) = delete;
		VertexBuffer(VertexBuffer&& other) noexcept = delete;
		VertexBuffer& operator=(const VertexBuffer& other) = delete;
		VertexBuffer& operator=(VertexBuffer&& other) noexcept = delete;

		[[nodiscard]] virtual uint32_t getVertexSize() = 0;
		
		virtual void write(const std::vector<uint8_t>& data) = 0;
	};
}
