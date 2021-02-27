#include "dt_controller.h"

#include <GLFW.h>

namespace digbuild::platform::desktop
{
	bool Controller::isConnected() const
	{
		return glfwJoystickPresent(m_id);
	}

	std::vector<bool> Controller::getButtonStates() const
	{
		if (!glfwJoystickPresent(m_id))
			return std::vector<bool>();
		
		int count;
		const auto* buttonStates = glfwGetJoystickButtons(m_id, &count);

		std::vector<bool> vector;
		vector.reserve(count);
		for (auto i = 0; i < count; ++i)
			vector.push_back(buttonStates[i]);

		return vector;
	}

	std::vector<float> Controller::getJoysticks() const
	{
		if (!glfwJoystickPresent(m_id))
			return std::vector<float>();
		
		int count;
		const auto* joystickStates = glfwGetJoystickAxes(m_id, &count);
		return std::vector(joystickStates, joystickStates + count);
	}

	std::vector<std::bitset<4>> Controller::getHatStates() const
	{
		if (!glfwJoystickPresent(m_id))
			return std::vector<std::bitset<4>>();
		
		int count;
		const auto* hatStates = glfwGetJoystickHats(m_id, &count);

		std::vector<std::bitset<4>> vector;
		vector.reserve(count);
		for (auto i = 0; i < count; ++i)
			vector.emplace_back(hatStates[i]);

		return vector;
	}
}
