# Cupscale
Image Upscaling GUI based on ESRGAN

![](https://i.imgur.com/ntIuSrv.png)

## Why fork?
Cupscale hasn't been updated for almost 1 year at the time of forking, but still has a lot of unexpected behavior/interface bugs that
that don't really make it unusable but do make it a bit annoying to deal with for daily use. I probably won't fix many backend issues
(outside of the RTX 3060 CUDA detection bug) but things like Cupscale throwing when a preview image isn't available even
when switching to a different tab, would be worked on.

This fork will be dormant while I study the repo to ensure I understand what's going on before I change things around and break stuff.
It's also more of a personal use fork so I won't make any garuntees on the extent to which I put energy into fixing non-breaking bugs.

## Credits:

Based around [xinntao's ESRGAN](https://github.com/xinntao/ESRGAN) implemented via [Joey's Fork](https://github.com/JoeyBallentine/ESRGAN).

AMD/Intel GPU compatibility is possible thanks to BlueAmulet's [esrgan-ncnn-vulkan](https://github.com/BlueAmulet/realsr-ncnn-vulkan) based on nihui's [realsr-ncnn-vulkan](https://github.com/nihui/realsr-ncnn-vulkan) running on Tencent's [ncnn](https://github.com/Tencent/ncnn) framework, as well as [xinntao's Real-ESRGAN](https://github.com/xinntao/Real-ESRGAN).

## Download:

[Get the latest release](https://github.com/Raynsauce/cupscale/releases)

## Installation:

The application is more or less portable. It's a single executable that you can run anywhere.

Temporary files are stored in the installation directory by default, which is why you shouldn't install the application in protected locations like Program Files.

## Supported AI Backends:

- Nvidia CUDA (Recommended)
- Vulkan (Works on any modern GPU, but is slower and takes a long time start up)
- CPU (Works without GPU, but is very slow)

## Features:

- CUDA, Vulkan/NCNN or CPU supported, with included model converter for NCNN
- On-the-fly Model Interpolation
- Model Chaining (Run images through multiple models at once)
- Batch Upscaling (Load a directory or multiple single images)
- Automatic Image tiling/merging to avoid running out of VRAM
- Pre-Processing: Optionally downscale images before upscaling
- Post-Processing: Automatically resize after upscaling
- Compatible with PNG, JPEG, BMP, WEBP, TGA, DDS images
- Load image straight out of the clipboard (no need to download images from web)
- Create various types of comparisons (Side-By-Side, 50/50, and before/after animations as GIF or MP4)
