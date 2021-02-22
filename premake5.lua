require "modules/digbuild"

workspace "DigBuildPlatform"
    configurations { "Debug", "Release" }
    startproject "DigBuildPlatformTest"

    include "PlatformCPP"
    include "PlatformCS"
    include "PlatformSourceGen"
    include "PlatformTest"
