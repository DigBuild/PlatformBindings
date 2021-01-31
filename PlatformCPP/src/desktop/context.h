#pragma once
#include <thread>

namespace digbuild::platform::desktop
{
	class GLFWContext final
	{
	public:
		GLFWContext(bool noApi);
		~GLFWContext();
		GLFWContext(const GLFWContext& other) = delete;
		GLFWContext(GLFWContext&& other) noexcept = delete;
		GLFWContext& operator=(const GLFWContext& other) = delete;
		GLFWContext& operator=(GLFWContext&& other) noexcept = delete;

	private:
		std::thread m_updateThread;
		bool m_terminate = false;

		friend class RenderSurface;
	};
}
