import http.client, urllib.request, urllib.parse, urllib.error, json

def FaceInfo(imageArray):
    print("Inicia la request")
    httpsConnection = ''
    headers = {
    'Content-Type': 'application/octet-stream',
    'Ocp-Apim-Subscription-Key': '',
    }

    params = urllib.parse.urlencode({
        "returnFaceId":"true",
        "returnFaceLandmarks":"false",
        "returnFaceAttributes":"age,gender,emotion"
    })
    imageData = []
    for frame in imageArray:
        imageInBytes = open(frame, "rb").read()
        try:
            print(frame)
            conn = http.client.HTTPSConnection(httpsConnection)
            conn.request("POST", "/face/v1.0/detect?%s" % params,imageInBytes, headers)
            response = conn.getresponse()
            data = response.read()
            jdata = json.loads(data)
            if len(jdata) > 0:
                jdata[0]["img"] = frame
                print(jdata)
                imageData.append(jdata[0])
            conn.close()
        except Exception as e:
            print(e)

    print("Fin de la request")
    return imageData