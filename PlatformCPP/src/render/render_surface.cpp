#include "render_surface.h"

#include "../util/native_handle.h"
#include "../util/utils.h"

using namespace digbuild::platform::input;
using namespace digbuild::platform::util;
using namespace digbuild::platform::render;
extern "C" {
	DLLEXPORT SurfaceInputContext* dbp_render_surface_get_input_context(const native_handle instance)
	{
		return &handle_cast<RenderSurface>(instance)->getInputContext();
	}
	
	DLLEXPORT uint32_t dbp_render_surface_get_width(const native_handle instance)
	{
		return handle_cast<RenderSurface>(instance)->getWidth();
	}

	DLLEXPORT uint32_t dbp_render_surface_get_height(const native_handle instance)
	{
		return handle_cast<RenderSurface>(instance)->getHeight();
	}

	DLLEXPORT void dbp_render_surface_get_title(const native_handle instance, void (*callback)(const char*))
	{
		callback(handle_cast<RenderSurface>(instance)->getTitle().c_str());
	}

	DLLEXPORT bool dbp_render_surface_is_fullscreen(const native_handle instance)
	{
		return handle_cast<RenderSurface>(instance)->isFullscreen();
	}

	DLLEXPORT bool dbp_render_surface_is_visible(const native_handle instance)
	{
		return handle_cast<RenderSurface>(instance)->isVisible();
	}

	DLLEXPORT bool dbp_render_surface_is_resized(const native_handle instance)
	{
		return handle_cast<RenderSurface>(instance)->isResized();
	}

	DLLEXPORT void dbp_render_surface_set_width(const native_handle instance, const uint32_t width)
	{
		handle_cast<RenderSurface>(instance)->setWidth(width);
	}

	DLLEXPORT void dbp_render_surface_set_height(const native_handle instance, const uint32_t height)
	{
		handle_cast<RenderSurface>(instance)->setHeight(height);
	}

	DLLEXPORT void dbp_render_surface_set_title(const native_handle instance, const char* title)
	{
		handle_cast<RenderSurface>(instance)->setTitle(title);
	}

	DLLEXPORT void dbp_render_surface_set_fullscreen(const native_handle instance, const bool fullscreen)
	{
		handle_cast<RenderSurface>(instance)->setFullscreen(fullscreen);
	}

	DLLEXPORT void dbp_render_surface_close(const native_handle instance)
	{
		handle_cast<RenderSurface>(instance)->close();
	}

	DLLEXPORT void dbp_render_surface_wait_closed(const native_handle instance)
	{
		handle_cast<RenderSurface>(instance)->waitClosed();
	}
}
