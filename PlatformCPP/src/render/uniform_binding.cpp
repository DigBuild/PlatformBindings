#include "uniform_binding.h"

#include "../util/native_handle.h"
#include "../util/utils.h"

using namespace digbuild::platform::util;
using namespace digbuild::platform::render;
extern "C"
{
	DLLEXPORT void dbp_uniform_binding_update(
		const native_handle instance,
		const native_handle buffer
	)
	{
		handle_cast<UniformBinding>(instance)->update(handle_share<UniformBuffer>(buffer));
	}
}
