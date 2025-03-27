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
        private Session session;
        public string AppId { get; }
        public byte[] AesKey { get; }
        private UniTask sessionTask;

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
            sessionTask = StartSesion()
             .ContinueWith(s =>
             {
                 session = s;
             });
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotId">0-indexed number of slot</param>
        /// <param name="saveData">json</param>
        /// <returns></returns>
        public async UniTask SaveSlot(int slotId, string saveData)
        {
            if (session == null)
            {
                Debug.LogError("SaveSlot Error : Session is Invalid");
                return;
            }

            DateTime now = DateTime.Now;
            if ((now - lastTimeSaved) < saveDelay)
            {
                await UniTask.Yield();
            }

            Request.ExecuteObject executeObj = NewExecuteObject("CloudSave.setData");
            executeObj.Parameters = new()
            {
                {"id",slotId },
                {"data",saveData }
            };
            lastTimeSaved = DateTime.Now;
            await SendRequest(executeObj);
        }

        public async UniTask<Session> GetSession()
        {
            await sessionTask;
            return session;
        }
        public async UniTask<string[]> LoadSlots()
        {
            await sessionTask;
            Response<List<SaveSlot>> resp = await SendRequest<List<SaveSlot>>(NewExecuteObject("CloudSave.loadSlots"));
            return resp.Result.Data["slots"].ConvertAll(s => s.Data).ToArray();
        }
        private async UniTask<Session> StartSesion()
        {

            Response<Session> res = await SendRequest<Session>(NewExecuteObject("App.startSession"));
            return res.Result.Data["session"];
        }


        private Request MakeRequest(Request.ExecuteObject executeObject, bool sesionID = false)
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