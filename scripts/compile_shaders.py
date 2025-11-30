#!/usr/bin/python
import os
import subprocess

os.chdir("../Riateu/Core/Misc")

shaders = []
for (dirpath, dirnames, filenames) in os.walk("Shaders"):
    if dirpath == "Shaders/common":
        continue
    shaders = filenames

for shader in shaders:
    filename, file_extension = os.path.splitext(shader)

    shader_file = "Shaders/" + filename + file_extension

    print("Compiling " + shader_file)

    vk_out = "Compiled/Vulkan/" + filename + ".spv"
    dx_out = "Compiled/DX12/" + filename + ".dxil"
    if file_extension == ".slang":
        subprocess.call(["slangc", shader_file, "-emit-spirv-via-glsl", "-O3", "-o", vk_out, "-entry", "main"])
        subprocess.call(["slangc", shader_file, "-profile", "sm_6_3", "-O3", "-o", dx_out, "-entry", "main"])