#include "dt_global_input_context.h"
#include <GLFW.h>

#include "dt_controller.h"

namespace digbuild::platform::desktop
{
	std::vector<std::shared_ptr<input::Controller>> GlobalInputContext::getControllers()
	{
		if (!m_initialized)
		{
			m_initialized = true;

			for (auto i = 0u; i < 16; ++i)
			{
				if (glfwJoystickPresent(static_cast<int>(i)))
				{
					auto guid = std::string(glfwGetJoystickGUID(i));
					m_controllers.push_back(std::make_shared<Controller>(i, guid));
				}
			}
		}
		return m_controllers;
	}
}
