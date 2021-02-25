#include "framebuffer.h"

#include "../util/native_handle.h"
#include "../util/utils.h"
#include "../util/vecmath.h"

using namespace digbuild::platform::util;
using namespace digbuild::platform::render;
extern "C" {
	DLLEXPORT uint32_t dbp_framebuffer_get_width(
		const native_handle instance
	)
	{
		return handle_cast<Framebuffer>(instance)->getWidth();
	}

	DLLEXPORT uint32_t dbp_framebuffer_get_height(
		const native_handle instance
	)
	{
		return handle_cast<Framebuffer>(instance)->getHeight();
	}

	DLLEXPORT native_handle dbp_framebuffer_get_texture(
		const native_handle instance,
		const uint32_t attachment
	)
	{
		return make_native_handle(handle_cast<Framebuffer>(instance)->getTexture(attachment));
	}
}

