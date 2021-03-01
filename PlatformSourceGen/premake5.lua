project "DigBuildPlatformSourceGen"
    kind "SharedLib"
    framework "netstandard2.0"
    language "C#"
    csversion "9.0"
	packageid "DigBuild.Platform.SourceGen"
	targetname "DigBuild.Platform.SourceGen"
    enabledefaultcompileitems(true)
    allownullable(true)
    noframeworktag(true)
    clr "Unsafe"
    targetdir "../bin/%{cfg.buildcfg}"
    objdir "../bin-int/%{cfg.buildcfg}"

    nuget {
		"Microsoft.CodeAnalysis.CSharp:3.8.0",
		"Microsoft.CodeAnalysis.Analyzers:3.3.2"
	}

    filter "configurations:Debug"
        defines { "DB_DEBUG" }
        symbols "On"
