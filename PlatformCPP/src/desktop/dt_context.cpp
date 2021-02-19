#include "dt_context.h"

#include <GLFW.h>

namespace digbuild::platform::desktop
{
	GLFWContext::GLFWContext(const bool noApi)
	{
		glfwInit();
		if (noApi)
			glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
	}

	GLFWContext::~GLFWContext()
	{
		glfwTerminate();
	}
}
