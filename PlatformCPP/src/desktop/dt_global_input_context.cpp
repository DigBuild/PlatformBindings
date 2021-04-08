#include "dt_global_input_context.h"
#include <algorithm>
#include <GLFW.h>

#include "dt_controller.h"

namespace digbuild::platform::desktop
{
	std::vector<std::shared_ptr<input::Controller>> GlobalInputContext::getControllers()
	{
		initialize();
		return m_controllers;
	}

	void GlobalInputContext::update()
	{
		initialize();
		
		// Remove disconnected controllers
		m_controllers.erase(
			std::remove_if(
				m_controllers.begin(), m_controllers.end(),
				[](const std::shared_ptr<input::Controller>& c)
				{
					return !std::static_pointer_cast<Controller>(c)->isConnected();
				}
			),
			m_controllers.end()
		);
	}

	void GlobalInputContext::initialize()
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
	}
}
