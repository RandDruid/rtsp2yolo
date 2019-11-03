# rtsp2yolo

This is a test project bringing together the RTSP video player and a real-time object detection system, written in C# (Visual Studio). RTSP code with FFMPEG support for decoding of H.264 frames was taken from [BogdanovKirill/RtspClientSharp](https://github.com/BogdanovKirill/RtspClientSharp). Object detection for the GPU was taken from [AlexeyAB/darknet](https://github.com/AlexeyAB/darknet). [Emgu.CV](https://github.com/emgucv/emgucv) was used for graphics manipulation.

My main idea was to bring all the "interesting" things to C# while leaving "heavy lifting" tasks to external DLLs. Source code is not perfect and I cut some corners extending the RTSP player, but this is a test project after all.

There is a clear point in the code where the user can do any additional processing on the image from the camera using Emgu.CV before passing it to detection.

## Features
#### What was changed in 'parent' projects:
- **Yolo/darknet:** I added a C function to `yolo_cpp_dll.dll` to directly pass pixel data to the detection functions, without the need to save the image in a file-like format to a stream or to disk. The changes are in [my fork of darknet](https://github.com/RandDruid/darknet). I included the already compiled library in this repo.
- **RTSP player:** I added another handler to "OnFrameReceived" to push video frames to a separate thread for detection of objects. The parameters list was extended to pass frame size to the detection function. I also included intermediary `libffmeghelper.dll` and the main FFMEG libraries in the repo.

#### Functionality I added:
 * display of detected entities as separate WPF objects, without direct drawing on the image itself.
 * object tracking - comparing object types and positions between different frames. The system assigns a unique ID to every new object and tries to follow it even if it moves. In my code I use "memory" for two sequential frames. It means that even if YOLO missed an object on the previous frame, there is still a chance that it was present 2 frames ago, and in that case the system will keep the same ID for this object.
 * basic statistics

## System requirements
- **.NET framework 4.6.2**
- **GPU with CC >= 3.0**: https://en.wikipedia.org/wiki/CUDA#GPUs_supported
- **CUDA 10.0**: https://developer.nvidia.com/cuda-toolkit-archive
- **cuDNN >= 7.0 for CUDA 10.0** https://developer.nvidia.com/rdp/cudnn-archive copy `cudnn64_7.dll` to `External\x64` solution folder.
- solution was created in Visual Studio 2019

Apart from cuDNN library mentioned above, I tried to put all required DLLs in the External folder of the repo, where VS will take it during compilation.

***Note: everything should be compiled for x64 !***

#### Pre-trained models
Models can be taken from [AlexeyAB/darknet](https://github.com/AlexeyAB/darknet). In the source code I use the `yolov3` [model](https://github.com/AlexeyAB/darknet/blob/master/README.md#pre-trained-models), which should be placed into the `Configs\yolov3\` folder. A model consists of a configuration file, a weights file and an object names file. Please take the weights file from the darknet page.

## Example results
I tested the project with an HD security camera, with 25 FPS 1920x1080 output. The video was sent over 1Gb Ethernet / RTSP / UDP.
For GPU I used GTX 1050 Ti with 4 GB of DDR5.
Using the [yolov3 model](https://github.com/AlexeyAB/darknet/blob/master/README.md#pre-trained-models) the detection thread managed to output objects at roughly 12 FPS, meaning every second frame was processed. Most of the time system easily tracked moving cars and people, despite the suboptimal angle of view for the model used.

![day](https://github.com/RandDruid/rtsp2yolo/blob/master/site/day.jpg)

![night](https://github.com/RandDruid/rtsp2yolo/blob/master/site/night.jpg)
