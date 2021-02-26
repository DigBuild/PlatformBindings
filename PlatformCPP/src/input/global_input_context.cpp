#include "global_input_context.h"

#include "../util/native_handle.h"
#include "../util/utils.h"

using namespace digbuild::platform::input;
using namespace digbuild::platform::util;
extern "C" {
	DLLEXPORT void dbp_global_input_context_initialize(
		GlobalInputContext* instance,
		void(*callback)(native_handle* controllers, uint32_t controllerCount)
	)
	{
		auto controllers = instance->getControllers();
		std::vector<native_handle> handles;
		handles.reserve(controllers.size());
		for (const auto& c : controllers)
			handles.push_back(make_native_handle<Controller>(c->shared_from_this()));
		callback(handles.data(), static_cast<uint32_t>(handles.size()));
	}
	
	DLLEXPORT void dbp_global_input_context_get_controller_guid(
		const native_handle instance,
		void(*callback)(const char* guid)
	)
	{
		const auto guid = handle_cast<Controller>(instance)->getGUID();
		callback(guid.c_str());
	}
	
	DLLEXPORT void dbp_global_input_context_get_controller_state(
		const native_handle instance,
		void(*callback)(uint8_t* buttonStates, uint32_t buttonCount, float* joystickStates, uint32_t joystickCount, uint8_t* hatStates, uint32_t hatCount)
	)
	{
		auto* controller = handle_cast<Controller>(instance);

		auto btns = controller->getButtonStates();
		std::vector<uint8_t> buttonStates;
		buttonStates.reserve(btns.size());
		for (const auto btn : btns)
			buttonStates.push_back(btn ? 1 : 0);

		auto joysticks = controller->getJoysticks();

		auto hats = controller->getHatStates();
		std::vector<uint8_t> hatStates;
		hatStates.reserve(hats.size());
		for (const auto hat : hats)
			hatStates.push_back(static_cast<uint8_t>(hat.to_ulong()));

		callback(
			buttonStates.data(), static_cast<uint32_t>(buttonStates.size()),
			joysticks.data(), static_cast<uint32_t>(joysticks.size()),
			hatStates.data(), static_cast<uint32_t>(hatStates.size())
		);
	}
}

