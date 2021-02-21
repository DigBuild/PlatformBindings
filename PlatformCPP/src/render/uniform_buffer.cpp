#include "uniform_buffer.h"

#include <vector>

#include "../util/native_handle.h"
#include "../util/utils.h"

using namespace digbuild::platform::util;
using namespace digbuild::platform::render;
extern "C"
{
	DLLEXPORT void dbp_uniform_buffer_write(
		const native_handle instance,
		const uint8_t* data,
		const uint32_t dataLength
	)
	{
		handle_cast<UniformBuffer>(instance)->write(std::vector(data, data + dataLength));
	}
}
