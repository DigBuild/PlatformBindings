project "DigBuildPlatformCS"
    kind "SharedLib"
    framework "net5.0"
    language "C#"
    csversion "9.0"
    enabledefaultcompileitems(true)
    allownullable(true)
    noframeworktag(true)
    clr "Unsafe"
    targetdir "../bin/%{cfg.buildcfg}"
    objdir "../bin-int/%{cfg.buildcfg}"

    dependson { "DigBuildPlatformCPP" }

    nuget {
        "AdvancedDLSupport:3.2.0",
        "System.Drawing.Common:5.0.1"
    }

    filter "configurations:Debug"
        defines { "DB_DEBUG" }
        symbols "On"
