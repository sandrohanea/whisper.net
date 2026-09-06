# CMakeList.txt : Top-level CMake project file, do global configuration
# and include sub-projects here.
#
cmake_minimum_required (VERSION 3.10)

project ("Whisper.net")

set(WHISPER_NET_RUNTIME_NAME "whisper" CACHE STRING "Suffix used for GGML runtime dependencies")
set_property(CACHE WHISPER_NET_RUNTIME_NAME PROPERTY STRINGS whisper parakeet)

if(NOT WHISPER_NET_RUNTIME_NAME STREQUAL "whisper" AND
   NOT WHISPER_NET_RUNTIME_NAME STREQUAL "parakeet")
    message(FATAL_ERROR "WHISPER_NET_RUNTIME_NAME must be either 'whisper' or 'parakeet'")
endif()

# Include sub-projects.
add_subdirectory ("whisper.cpp")

if(TARGET ggml)
    set_target_properties(ggml PROPERTIES OUTPUT_NAME "ggml-${WHISPER_NET_RUNTIME_NAME}")
endif()

if(TARGET ggml-cpu)
    set_target_properties(ggml-cpu PROPERTIES OUTPUT_NAME "ggml-cpu-${WHISPER_NET_RUNTIME_NAME}")
endif()

if(TARGET ggml-base)
    set_target_properties(ggml-base PROPERTIES OUTPUT_NAME "ggml-base-${WHISPER_NET_RUNTIME_NAME}")
endif()

if(TARGET ggml-cuda)
    set_target_properties(ggml-cuda PROPERTIES OUTPUT_NAME "ggml-cuda-${WHISPER_NET_RUNTIME_NAME}")
endif()

if(TARGET ggml-vulkan)
    set_target_properties(ggml-vulkan PROPERTIES OUTPUT_NAME "ggml-vulkan-${WHISPER_NET_RUNTIME_NAME}")
endif()

if(TARGET ggml-metal)
    set_target_properties(ggml-metal PROPERTIES OUTPUT_NAME "ggml-metal-${WHISPER_NET_RUNTIME_NAME}")
endif()

if(TARGET ggml-blas)
    set_target_properties(ggml-blas PROPERTIES OUTPUT_NAME "ggml-blas-${WHISPER_NET_RUNTIME_NAME}")
endif()

if (CMAKE_SYSTEM_NAME STREQUAL "Android")
  # Set alignment to 16 KB. See https://developer.android.com/guide/practices/page-sizes#cmake
  target_link_options(ggml PRIVATE "-Wl,-z,max-page-size=16384")
  target_link_options(whisper PRIVATE "-Wl,-z,max-page-size=16384")
  if(TARGET parakeet)
    target_link_options(parakeet PRIVATE "-Wl,-z,max-page-size=16384")
  endif()
  target_link_options(ggml-cpu PRIVATE "-Wl,-z,max-page-size=16384")
  target_link_options(ggml-base PRIVATE "-Wl,-z,max-page-size=16384")
endif()
