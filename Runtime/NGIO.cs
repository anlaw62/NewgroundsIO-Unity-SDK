using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Newgrounds
{
    public class NGIO
    {
        public static NGIO Instance { get; private set; }
        public Session Session { get; private set; }
        public string AppId { get; }
        public byte[] AesKey { get; }
        internal const string GATEWAY_URI = "https://www.newgrounds.io/gateway_v3.php";

        public NGIO(string appId, string aesKey)
        {
            if (Instance != null)
            {
                Debug.LogError("Attempt of creating second ngio instance");
                return;
            }
            Instance = this;
            AppId = appId;
            AesKey = Encoding.UTF8.GetBytes(aesKey);
            StartSesion()
            .ContinueWith(s =>
            {
                Session = s;
            }).Forget();
            pingWebRequest = MakeWebRequest(NewExecuteObject("Gateway.ping"));

        }
        private UnityWebRequest pingWebRequest;
        private DateTime lastTimePing;
        private DateTime lastTimeSaved;
        private readonly TimeSpan saveDelay = TimeSpan.FromSeconds(4);
        private readonly TimeSpan timePingDelay = TimeSpan.FromMinutes(3);
        public async UniTask Ping()
        {
            DateTime now = DateTime.Now;
            if ((now - lastTimePing) > timePingDelay)
            {
                lastTimePing = now;
                await pingWebRequest.SendWebRequest().ToUniTask();

            }
        }
        public async UniTask SaveSlot(int slotId, string saveData)
        {
            DateTime now = DateTime.Now;
            if((now - lastTimeSaved) < saveDelay)
            {
                await UniTask.Yield();
            }

            Request.ExecuteObject executeObj = NewExecuteObject("CloudSave.setData");
            executeObj.Parameters = new()
            {
                {"id",slotId },
                {"data",saveData }
            };
            executeObj.Encrypt(AesKey);
            lastTimeSaved = DateTime.Now;
            await SendRequest(executeObj);
        }

        private async UniTask<Session> StartSesion()
        {

            Response<Session> res = await SendRequest<Session>(NewExecuteObject("App.startSession"));
            return res.Result.Data["session"];
        }


        private Request MakeRequest(Request.ExecuteObject executeObject)
        {
            return new() { AppId = AppId, ExecuteObj = executeObject };
        }
        private Request.ExecuteObject NewExecuteObject(string component)
        {
            return new() { Component = component };
        }
        private UnityWebRequest MakeWebRequest(Request request)
        {
            UnityWebRequest webRequest = new(GATEWAY_URI, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonSerialization.ToJson(request))),

            };
            webRequest.SetRequestHeader("Content-Type", "application/json");
            return webRequest;
        }
        private UnityWebRequest MakeWebRequest(Request.ExecuteObject executeObject)
        {
            return MakeWebRequest(MakeRequest(executeObject));
        }


        private async UniTask SendRequest(Request request)
        {
            UnityWebRequest webRequest = MakeWebRequest(request);


            await webRequest.SendWebRequest().ToUniTask();
        }
        private async UniTask<Response<ResultDataType>> SendRequest<ResultDataType>(Request.ExecuteObject executeObject)
        {
            Request request = MakeRequest(executeObject);
            return await SendRequest<ResultDataType>(request);
        }
        private async UniTask SendRequest(Request.ExecuteObject executeObject)
        {
            Request request = MakeRequest(executeObject);
            await SendRequest(request);
        }


        private async UniTask<Response<ResultDataType>> SendRequest<ResultDataType>(Request request)
        {
            UnityWebRequest webRequest = MakeWebRequest(request);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            await webRequest.SendWebRequest().ToUniTask();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Send Request Error");
                return default;
            }
            else
            {
                string resJson = webRequest.downloadHandler.text;
                Response<ResultDataType> response = JsonSerialization.FromJson<Response<ResultDataType>>(resJson);
                if (!response.Success)
                {
                    Debug.LogError(response.Error.Message);
                }

                return response;
            }

        }
    }
}