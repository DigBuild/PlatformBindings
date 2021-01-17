#include "context.h"

#include <GLFW.h>

#include "render_surface.h"

namespace digbuild::platform::desktop
{
	GLFWContext::GLFWContext(const bool noApi)
	{
		glfwInit();
		if (noApi)
			glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
		
		m_updateThread = std::thread(
			[&]()
			{
				while (!m_terminate)
					glfwWaitEvents();
			}
		);
	}

	GLFWContext::~GLFWContext()
	{
		m_terminate = true;
		glfwPostEmptyEvent();
		
		if (m_updateThread.joinable())
			m_updateThread.join();
		
		glfwTerminate();
	}
}
