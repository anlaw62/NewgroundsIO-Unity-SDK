using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Newgrounds
{
    public class NGIO
    {
        public Session Session {get; private set;}
        public string AppId { get; private set; }
        public string AesKey { get; private set; }
        internal const string GATEWAY_URI = "https://www.newgrounds.io/gateway_v3.php";
        public NGIO(string appId, string aesKey)
        {
            AppId = appId;
            AesKey = aesKey;
            StartSesion()
            .ContinueWith(s =>
            {
                UnityEngine.Debug.Log(s);
                Session = s;
            }).Forget();

       
        }
        private async UniTask<Session> StartSesion()
        {

            Response<Session> res = await SendRequest<Session>(NewExecuteObject("App.startSession"));
            return res.Result.Data["session"];
        }

        private async UniTask<Response<ResultDataType>> SendRequest<ResultDataType>(Request.ExecuteObject executeObject)
        {
            Request request = new() { AppId = AppId };
            request.ExecuteObj = executeObject;
          return await  SendRequest<ResultDataType>(request);
        }
    
    
        private Request.ExecuteObject NewExecuteObject(string component)
        {
            return new() { Component = component};
        }
        private async UniTask<Response<ResultDataType>> SendRequest<ResultDataType>(Request request)
        {
            UnityWebRequest webRequest = new(GATEWAY_URI, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonSerialization.ToJson(request))),
                downloadHandler = new DownloadHandlerBuffer()
            };

            webRequest.SetRequestHeader("Content-Type", "application/json");


            await webRequest.SendWebRequest().ToUniTask();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Send Request Error");
                return default;
            }
            else
            {
                string resJson = webRequest.downloadHandler.text;
                Response< ResultDataType> response = JsonSerialization.FromJson<Response<ResultDataType>>(resJson);
                if (!response.Success)
                {
                    Debug.LogError(response.Error.Message);
                }
            
                return response;
            }

        }
    }
}