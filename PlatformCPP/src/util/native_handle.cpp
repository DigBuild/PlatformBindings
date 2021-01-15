#include "native_handle.h"

#include "utils.h"

using namespace digbuild::platform::util;
extern "C"
{
	DLLEXPORT void dbp_native_handle_destroy(Destructible* handle)
	{
		delete handle;
	}
}
