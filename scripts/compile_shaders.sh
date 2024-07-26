#!/usr/bin/bash
cd ../Riateu/Core/Misc

for filename in Shaders/*.{vert,comp,frag}; do
    name=$(basename $filename)
    vk_out="Compiled/$name.spv"

    echo "Compiling Shaderes..."
    glslang $filename -V -o $vk_out
done
