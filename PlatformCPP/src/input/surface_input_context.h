#pragma once
#include <functional>

namespace digbuild::platform::input
{
	enum class KeyboardAction : uint8_t
	{
		RELEASE,
		PRESS,
		REPEAT,
		CHARACTER
	};
	enum class MouseAction : uint8_t
	{
		RELEASE,
		PRESS
	};
	enum class CursorAction : uint8_t
	{
		MOVE,
	};
	enum CursorMode : uint8_t
	{
		NORMAL,
		HIDDEN,
		RAW
	};
	
	using KeyboardEventConsumer = std::function<void(uint32_t code, KeyboardAction action)>;
	using MouseEventConsumer = std::function<void(uint32_t button, MouseAction action)>;
	using ScrollEventConsumer = std::function<void(double xOffset, double yOffset)>;
	using CursorEventConsumer = std::function<void(uint32_t x, uint32_t y, CursorAction action)>;
	
	class SurfaceInputContext
	{
	public:
		SurfaceInputContext() = default;
		virtual ~SurfaceInputContext() = default;
		SurfaceInputContext(const SurfaceInputContext& other) = delete;
		SurfaceInputContext(SurfaceInputContext&& other) noexcept = delete;
		SurfaceInputContext& operator=(const SurfaceInputContext& other) = delete;
		SurfaceInputContext& operator=(SurfaceInputContext&& other) noexcept = delete;

		virtual void consumeKeyboardEvents(KeyboardEventConsumer consumer) = 0;
		virtual void consumeMouseEvents(MouseEventConsumer consumer) = 0;
		virtual void consumeScrollEvents(ScrollEventConsumer consumer) = 0;
		virtual void consumeCursorEvents(CursorEventConsumer consumer) = 0;
		
		[[nodiscard]] virtual CursorMode getCursorMode() const = 0;
		virtual void setCursorMode(CursorMode mode) = 0;
		virtual void centerCursor() = 0;
	};
}
