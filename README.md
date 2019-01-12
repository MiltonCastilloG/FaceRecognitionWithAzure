# FaceRecognitionWithAzure

One Paragraph of project description goes here

## Getting Started

Clone the proyect into whatever carpet you want

### Prerequisites

Python ^3.5
C# with Visual Studio 2017
Lastest version of RabbitMQ

### Installing

pip install pika
pip install cv2
pip install numpy

## Deployment

1. Open the FrameAnalyzer.py archive and enter you Azure https connection for the Face API on the "httpsConnection" variable, then enter your subscription key on "headers['Ocp-Apim-Subscription-Key']
2. On the ImageProcessing forlder open the command line and run the command "python rpc_server.py"
3. Then you can run the C# proyect on visual studio and once have a video it will be send automaticallyto the python server for processing.
