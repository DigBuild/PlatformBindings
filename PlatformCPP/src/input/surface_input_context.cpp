#include "surface_input_context.h"

#include "../util/utils.h"

using namespace digbuild::platform::input;
extern "C" {
	DLLEXPORT void dbp_surface_input_context_consume_keyboard_events(
		SurfaceInputContext* instance,
		void(*callback)(uint32_t scancode, KeyboardAction action)
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
	
	DLLEXPORT void dbp_surface_input_context_consume_cursor_events(
		SurfaceInputContext* instance,
		void(*callback)(uint32_t x, uint32_t y, CursorAction action)
	)
	{
		instance->consumeCursorEvents(callback);
	}
}

