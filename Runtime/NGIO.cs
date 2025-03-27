using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using UnityEngine.Networking;

namespace Newgrounds
{
    public class NGIO
    {
        public static NGIO Instance { get; private set; }
        private Session session;
        public string AppId { get; }
        public byte[] AesKey { get; }
        private readonly UniTaskCompletionSource sessionTaskSource;
        private JsonSerializerSettings serializerSettings;

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
            AesKey = Convert.FromBase64String(aesKey);
            serializerSettings = new()
            {
                Error = (object o, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
                            {
                                args.ErrorContext.Handled = true;

                            },
                NullValueHandling = NullValueHandling.Ignore
            };
            sessionTaskSource = new();
            StartSesion()
            .ContinueWith(s =>
            {
                session = s;
                sessionTaskSource.TrySetResult();
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
        public async UniTask<Medal[]> GetMedals()
        {
            Response<Medal[]> resp = await SendRequest<Medal[]>(NewExecuteObject("Medal.getList"));
            return resp.Result.Data["medals"];
        }
        public async UniTask UnlockMedal(int id)
        {
            await GetSession();
            Request.ExecuteObject executeObject = NewExecuteObject("Medal.unlock");
            executeObject.Parameters = new()
            {
                {"id",id }
            };
            executeObject.Encrypt(AesKey,serializerSettings);
          await SendRequest(executeObject);
         
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotId">0-indexed number of slot</param>
        /// <param name="saveData">json</param>
        /// <returns></returns>
        public async UniTask SaveSlot(int slotId, string saveData)
        {
            slotId += 1;
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
            Debug.LogError("Savly nax");
            await SendRequest(executeObj);
        }

        public async UniTask<Session> GetSession()
        {

            await sessionTaskSource.Task;

            return session;
        }        /// <summary>
                 /// 
                 /// </summary>
                 /// <param name="slotId">0-indexed number of slot</param>
                 /// <returns></returns>
        public async UniTask<string> LoadSlot(int slotId)
        {
            await GetSession();
            slotId += 1;
            Request.ExecuteObject executeObject = NewExecuteObject("CloudSave.loadSlot");
            executeObject.Parameters= new() { {"id",slotId} };
            Response<SaveSlot> resp = await SendRequest<SaveSlot>(executeObject);
            return await LoadSlot(resp.Result.Data["slot"]);
        }
        public async UniTask<string[]> LoadSlots()
        {
            await GetSession();
            Response<SaveSlot[]> resp = await SendRequest<SaveSlot[]>(NewExecuteObject("CloudSave.loadSlots"));

            SaveSlot[] slots = resp.Result.Data["slots"];
            string[] res = new string[slots.Length];
            for (int i = 0; i < res.Length; i++)
            {
                try
                {
                    UnityEngine.Debug.Log(slots[i].Url);
                    res[i] = await LoadSlot(slots[i]);

                }
                catch { }
            }
            return res;
        }
        private async UniTask<string> LoadSlot(SaveSlot slot)
        {
            if (slot.Url == null)
            {
                return null;
            }
            using (UnityWebRequest webRequest = new(slot.Url, "GET") { downloadHandler = new DownloadHandlerBuffer() })
            {

                await webRequest.SendWebRequest().ToUniTask();
                return webRequest.downloadHandler.text;
            }
        }
        private async UniTask<Session> StartSesion()
        {

            Response<Session> res = await SendRequest<Session>(NewExecuteObject("App.startSession"));
            return res.Result.Data["session"];
        }


        private Request MakeRequest(Request.ExecuteObject executeObject)
        {

            return new() { AppId = AppId, ExecuteObj = executeObject, SessionId = session?.Id };
        }
        private Request.ExecuteObject NewExecuteObject(string component)
        {
            return new() { Component = component };
        }
        private UnityWebRequest MakeWebRequest(Request request)
        {
       
            UnityWebRequest webRequest = new(GATEWAY_URI, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request,serializerSettings))),

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
           
                Response<ResultDataType> response = JsonConvert.DeserializeObject<Response<ResultDataType>>(resJson, serializerSettings);
                if (!response.Success)
                {
                    Debug.LogError(response.Error.Message);
                }

                return response;
            }

        }
    }
}