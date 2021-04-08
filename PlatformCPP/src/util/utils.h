#pragma once

#if defined(_MSC_VER)
#define DLLEXPORT __declspec(dllexport)
#elif defined(__GNUC__)
#define DLLEXPORT __attribute__((visibility("default")))
#else
#define DLLEXPORT
#endif
