#include "vertex_buffer.h"

#include <vector>

#include "../util/native_handle.h"
#include "../util/utils.h"

using namespace digbuild::platform::util;
using namespace digbuild::platform::render;
extern "C"
{
	DLLEXPORT void dbp_vertex_buffer_write(
		const native_handle instance,
		const uint8_t* data,
		const uint32_t vertexCount
	)
	{
		auto buf = handle_cast<VertexBuffer>(instance);
		buf->write(std::vector(data, data + (vertexCount * buf->getVertexSize())));
	}
}
