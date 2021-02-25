#include "texture.h"

#include "../util/native_handle.h"
#include "../util/utils.h"

using namespace digbuild::platform::util;
using namespace digbuild::platform::render;
extern "C"
{
	DLLEXPORT uint32_t dbp_texture_get_width(
		const native_handle instance
	)
	{
		return handle_cast<Texture>(instance)->getWidth();
	}
	DLLEXPORT uint32_t dbp_texture_get_height(
		const native_handle instance
	)
	{
		return handle_cast<Texture>(instance)->getHeight();
	}
}
