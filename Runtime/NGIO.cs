using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

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

#if !UNITY_EDITOR
            session = GetSessionFromUrl();
            sessionTaskSource.TrySetResult();
#endif
            pingWebRequest = MakeWebRequest(NewExecuteObject("Gateway.ping"));

        }
        private UnityWebRequest pingWebRequest;
        private DateTime lastTimePing;
        private DateTime lastTimeSaved;
        private readonly TimeSpan saveDelay = TimeSpan.FromSeconds(4);
        private readonly TimeSpan timePingDelay = TimeSpan.FromMinutes(3);
        [Preserve]
        private Session GetSessionFromUrl()
        {
            Uri uri = new(Application.absoluteURL);
            Dictionary<string, string> uriParams = URIParamsUtility.GetParams(uri);
            if (uriParams.ContainsKey("ngio_session_id"))
            {
                return new()
                {
                    Id = uriParams["ngio_session_id"],
                    User = new()
                    {
                        Id = int.Parse(uriParams["NewgroundsAPI_UserID"]),
                        Name = uriParams["ng_username"],
                    }
                }; 
            }
            return new();
        }
        public async UniTask Ping()
        {
            DateTime now = DateTime.Now;
            if ((now - lastTimePing) > timePingDelay)
            {
                lastTimePing = now;
                await pingWebRequest.SendWebRequest().ToUniTask();

            }
        }
        public async UniTask PostScore(int leaderboardId, int value, string tag = null)
        {
            await GetSession();
            Request.ExecuteObject executeObject = NewExecuteObject("ScoreBoard.postScore");
            executeObject.Parameters = new()
            {
                {"id",leaderboardId },
                {"value",value },
                {"tag",tag }
            };
            executeObject.Encrypt(AesKey, serializerSettings);
            await SendRequest(executeObject);
        }
        public async UniTask<Score[]> GetScores(int leaderboardId, int limit, Period period = Period.Year, int skip = 0, bool social = false, int userId = 0, string tag = null)
        {
            Request.ExecuteObject executeObject = NewExecuteObject("ScoreBoard.getScores");
            string periodStr;
            switch (period)
            {

                case Period.Month:
                    periodStr = "N";
                    break;
                case Period.Day:
                    periodStr = "D";
                    break;
                case Period.Week:
                    periodStr = "S";
                    break;
                default:
                    periodStr = "Y";
                    break;
            }
            executeObject.Parameters = new()
            {
                {"id",leaderboardId },
                {"period",periodStr },
                {"skip",skip },
                {"limit",limit},
                {"social",social },
                {"tag",tag },
                {"userId",userId}
            };
            executeObject.Encrypt(AesKey, serializerSettings);
            Response<Score[]> resp = await SendRequest<Score[]>(executeObject);
            return resp.Result.Data["scores"];
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
            executeObject.Encrypt(AesKey, serializerSettings);
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
            executeObj.Encrypt(AesKey, serializerSettings);
            await SendRequest(executeObj);
        }

        public async UniTask<Session> GetSession()
        {

            await sessionTaskSource.Task;

            return session;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotId">0-indexed number of slot</param>
        /// <returns></returns>
        public async UniTask<string> LoadSlot(int slotId)
        {
            await GetSession();
            slotId += 1;
            Request.ExecuteObject executeObject = NewExecuteObject("CloudSave.loadSlot");
            executeObject.Parameters = new() { { "id", slotId } };
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
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request, serializerSettings))),

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