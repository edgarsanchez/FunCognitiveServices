#r @"packages\FSharp.Data\lib\portable-net45+netcore45\FSharp.Data.dll"
#r "System.Globalization"
#r "System.Net.Primitives"

open FSharp.Data
open FSharp.Data.HttpRequestHeaders
open FSharp.Data.HttpContentTypes

type VisionApi = JsonProvider< @"resources\visionApiResult.json" >
let analyzeImage subscriptionKey imageUrl =
    let visionApiRequest = "https://api.projectoxford.ai/vision/v1.0/analyze?visualFeatures=Description,Tags"
    let response = Http.RequestString(
                    url = visionApiRequest, 
                    headers = [ "ocp-apim-subscription-key", subscriptionKey; ContentType Json ], 
                    body = (imageUrl |> sprintf """{"url": "%s"}""" |> HttpRequestBody.TextRequest) )
    let jsonObject = VisionApi.Parse response
    jsonObject.Description.Captions.[0].Text

let getCognitiveAuthToken subscriptionKey =
    let cognitiveTokenRequest = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken/"
    let response = Http.RequestString(
                    url = cognitiveTokenRequest,
                    headers = [ "ocp-apim-subscription-key", subscriptionKey],
                    body = TextRequest "" )
    response


type TranslationXml = XmlProvider< """<string xmlns="http://schemas.microsoft.com/2003/10/Serialization/">¡Hola mundo!</string>""" >
let translateText authToken fromLang toLang text =
    let translateRequest = "https://api.microsofttranslator.com/v2/http.svc/Translate"
    let response = Http.RequestString(
                    url = translateRequest,
                    query = [ "text", text
                              "from", fromLang
                              "to", toLang ], 
                    headers = [ "Authorization", "Bearer " + authToken ] )
    TranslationXml.Parse response

let toSpeech authToken lang text =
    let toSpeechRequest = "http://api.microsofttranslator.com/V2/Http.svc/Speak"
    let response = Http.Request(
                    url = toSpeechRequest, 
                    query = [ "text", text; "language", lang ],
                    headers = [ "Authorization", "Bearer " + authToken ] )
    match response.Body with
    | HttpResponseBody.Binary bytes -> Some bytes
    | _ -> None

open System.IO
open System.Media

let play (maybeBytes: byte[] option) =
    match maybeBytes with
    | Some bytes ->
        use stream = new MemoryStream(bytes)
        use player = new SoundPlayer(stream)
        player.PlaySync()
    | _ -> failwith "No bytes to sound."

let analyzerKey = "Your-Vision-API-Subscription-Key"
let translatorKey = "Your-Translation-API-Subscription-Key"
let translatorToken = getCognitiveAuthToken translatorKey
let imageUrl = "https://upload.wikimedia.org/wikipedia/commons/2/21/Ecuador_Chimborazo_5923.jpg"

imageUrl |> analyzeImage analyzerKey |> translateText translatorToken "en" "es" |> toSpeech translatorToken "es-MX" |> play
