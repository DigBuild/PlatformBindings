#include "surface_input_context.h"

#include "../util/utils.h"

using namespace digbuild::platform::input;
extern "C" {
	DLLEXPORT void dbp_surface_input_context_consume_keyboard_events(
		SurfaceInputContext* instance,
		void(*callback)(uint32_t code, KeyboardAction action)
	)
	{
		instance->consumeKeyboardEvents(callback);
	}
	
	DLLEXPORT void dbp_surface_input_context_consume_mouse_events(
		SurfaceInputContext* instance,
		void(*callback)(uint32_t button, MouseAction action)
	)
	{
		instance->consumeMouseEvents(callback);
	}
	
	DLLEXPORT void dbp_surface_input_context_consume_scroll_events(
		SurfaceInputContext* instance,
		void(*callback)(double xOffset, double yOffset)
	)
	{
		instance->consumeScrollEvents(callback);
	}
	
	DLLEXPORT void dbp_surface_input_context_consume_cursor_events(
		SurfaceInputContext* instance,
		void(*callback)(uint32_t x, uint32_t y, CursorAction action)
	)
	{
		instance->consumeCursorEvents(callback);
	}

	DLLEXPORT CursorMode dbp_surface_input_context_get_cursor_mode(
		SurfaceInputContext* instance
	)
	{
		return instance->getCursorMode();
	}

	DLLEXPORT void dbp_surface_input_context_set_cursor_mode(
		SurfaceInputContext* instance,
		const CursorMode mode
	)
	{
		instance->setCursorMode(mode);
	}

	DLLEXPORT void dbp_surface_input_context_center_cursor(
		SurfaceInputContext* instance
	)
	{
		instance->centerCursor();
	}
}

