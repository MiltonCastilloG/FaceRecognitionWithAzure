import cv2
import glob
import numpy as np
import os

def ImageToVideo(data, paths, newPath):
    imgs = []
    if len(data) > 0:
        nameSplit = newPath.split("\\videos\\")
        try:
            if not os.path.exists(nameSplit[0] + "\\newVideos"):
                os.makedirs(nameSplit[0] + "\\newVideos")
        except OSError:
            print ('Error: Creating directory of data')
        newVideoPath = nameSplit[0] + "\\newVideos\\" + nameSplit[1]
        for i, faceData in enumerate(data):
            #rectPos = [384, 80]
            #rectSize = [510, 128]

            y = faceData['faceRectangle']['top']
            x = faceData['faceRectangle']['left']
            x2 = x + faceData['faceRectangle']['width']
            y2 = y + faceData['faceRectangle']['height']
            rectColor = (0,255,0)
            textColor = (255,255,255)
            print("works")


            predEmotion = ""
            predValue = 0.0
            for key, value in faceData['faceAttributes']['emotion'].items():
                if value > predValue:
                    predEmotion = key

            img = cv2.imread(faceData["img"])
            cv2.rectangle(img,(x,y),(x2,y2),rectColor,3)
            cv2.putText(img, predEmotion, (x,y2+15), cv2.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 2)
            cv2.putText(img, "Age: "+str(faceData['faceAttributes']['age']), (x,y-15), cv2.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 2)
            cv2.putText(img, "Sex: "+str(faceData['faceAttributes']['gender']), (x2-15,y-15), cv2.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 2)
            imgs.append(img)
            
        height, width, layers =  imgs[0].shape
        video = cv2.VideoWriter(newVideoPath,-1,2.5,(width,height))

        for frame in imgs:
            video.write(frame)

        cv2.destroyAllWindows()
        video.release()
        return newVideoPath
    else:
        return "No face"

def VideoSplitter(fullPath):
    FPS = 25
    path = str(fullPath)
    cap = cv2.VideoCapture(path)
    cap.set(cv2.CAP_PROP_FPS, FPS)
    nameSplit = fullPath.split("\\videos\\")
    videoName = nameSplit[1]
    try:
        if not os.path.exists('data\\'+videoName):
            os.makedirs('data\\'+videoName)
    except OSError:
        print ('Error: Creating directory of data')

    newFramesPath = []
    currentFrame = 0
    writtenFrames = 0
    print('Separating Frames...')
    while(True):
        ret, frame = cap.read()

        if frame is None:
            break

        if currentFrame%12 == 0:
            name = '.\\data\\'+videoName+'\\' + str(writtenFrames).zfill(7) + '.jpg'
            newFramesPath.append(name)
            cv2.imwrite(name, frame)
            writtenFrames += 1
        currentFrame += 1
    print('Separating Frames DONE')

    cap.release()
    cv2.destroyAllWindows()
    return newFramesPath