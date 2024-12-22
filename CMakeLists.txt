# CMakeList.txt : Top-level CMake project file, do global configuration
# and include sub-projects here.
#
cmake_minimum_required (VERSION 3.8)

# Enable Hot Reload for MSVC compilers if supported.
if (POLICY CMP0141)
  cmake_policy(SET CMP0141 NEW)
  set(CMAKE_MSVC_DEBUG_INFORMATION_FORMAT "$<IF:$<AND:$<C_COMPILER_ID:MSVC>,$<CXX_COMPILER_ID:MSVC>>,$<$<CONFIG:Debug,RelWithDebInfo>:EditAndContinue>,$<$<CONFIG:Debug,RelWithDebInfo>:ProgramDatabase>>")
endif()

project ("Whisper.net")

# Include sub-projects.
add_subdirectory ("whisper.cpp")

if(TARGET ggml)
    set_target_properties(ggml PROPERTIES OUTPUT_NAME "ggml-whisper")
endif()

if(TARGET ggml-cpu)
    set_target_properties(ggml-cpu PROPERTIES OUTPUT_NAME "ggml-cpu-whisper")
endif()

if(TARGET ggml-base)
    set_target_properties(ggml-base PROPERTIES OUTPUT_NAME "ggml-base-whisper")
endif()

if(TARGET ggml-cuda)
    set_target_properties(ggml-cuda PROPERTIES OUTPUT_NAME "ggml-cuda-whisper")
endif()

if(TARGET ggml-vulkan)
    set_target_properties(ggml-vulkan PROPERTIES OUTPUT_NAME "ggml-vulkan-whisper")
endif()

if(TARGET ggml-metal)
    set_target_properties(ggml-metal PROPERTIES OUTPUT_NAME "ggml-metal-whisper")
endif()

if(TARGET ggml-blas)
    set_target_properties(ggml-blas PROPERTIES OUTPUT_NAME "ggml-blas-whisper")
endif()

if (CMAKE_SYSTEM_NAME STREQUAL "Android")
  # Set alignment to 16 KB. See https://developer.android.com/guide/practices/page-sizes#cmake
  target_link_options(ggml PRIVATE "-Wl,-z,max-page-size=16384")
  target_link_options(whisper PRIVATE "-Wl,-z,max-page-size=16384")
endif()
