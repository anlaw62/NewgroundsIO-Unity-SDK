using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Newgrounds
{
    public static class NGIO
    {
   
        private static Session session;
        public static string AppId { get; private set; }
        public static byte[] AesKey { get; private set; }
        private static UniTaskCompletionSource sessionTaskSource;
        private static JsonSerializerSettings serializerSettings;
        internal const string GATEWAY_URI = "https://www.newgrounds.io/gateway_v3.php";

        
     
        public static void Init(string appId, string aesKey, string sessionId = null)
        {
         
            AppId = appId;
            AesKey = Convert.FromBase64String(aesKey);

            serializerSettings = new()
            {
                Error = (object o, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
                {
                    args.ErrorContext.Handled = true;
                    UnityEngine.Debug.LogException(args.ErrorContext.Error);
                },
                NullValueHandling = NullValueHandling.Ignore
            };
            sessionTaskSource = new();
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(sessionId))
            {
                session = new() { Id = sessionId };
            }
#else

    session = GetSessionFromUrl();
#endif
            sessionTaskSource.TrySetResult();
           
            CreatePinger();
       
        }
        public static bool IsValidSession
        {
            get
            {
                return session != null;
            }
        }
        private static void CreatePinger()
        {
            if (Application.isPlaying)
            {
                GameObject pingerGo = new("NG Pinger");
                UnityEngine.Object.DontDestroyOnLoad(pingerGo);
                pingerGo.AddComponent<NGPinger>();
            }
        }
     
        private static DateTime lastTimeSaved;
        private static readonly TimeSpan saveDelay = TimeSpan.FromSeconds(4);
   
        [Preserve]
        private static Session GetSessionFromUrl()
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
            return null;
        }
        public static async UniTask Ping()
        {
            if (!IsValidSession)
            {
                return;
            }
         
               
                SendRequest<string>("Gateway.ping").Forget();

        }
        public static async UniTask PostScore(int leaderboardId, int value, string tag = null)
        {
            await GetSession();
            if (!IsValidSession)
            {
                Debug.LogError("Cant post score without session");
                return;
            }
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
        public static async UniTask<Score[]> GetScores(int leaderboardId, int limit, Period period = Period.Year, int skip = 0, bool social = false, int userId = 0, string tag = null)
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
            return resp.Result["scores"];
        }
        public static async UniTask<Medal[]> GetMedals()
        {
            Response<Medal[]> resp = await SendRequest<Medal[]>("Medal.getList");
            return resp.Result["medals"];
        }
        public static async UniTask UnlockMedal(int id)
        {
            await GetSession();
            if (!IsValidSession)
            {
                Debug.LogError("Cant unlock medal without session");
                return;
            }
            Request.ExecuteObject executeObject = NewExecuteObject("Medal.unlock");
            executeObject.Parameters = new()
            {
                {"id",id }
            };
            executeObject.Encrypt(AesKey, serializerSettings);
            await SendRequest(executeObject);
            Debug.Log($"unlockingAchievment {id}");
        }

        private static string saveDataToSet;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotId">0-indexed number of slot</param>
        /// <param name="saveData">json</param>
        /// <returns></returns>
        public static async UniTask SaveSlot(int slotId, string saveData)
        {
            if (!IsValidSession)
            {

                Debug.LogError("Cant saveslot without session");
                return;
            }
            slotId += 1;

            saveDataToSet = saveData;
            DateTime now = DateTime.Now;
            while ((now - lastTimeSaved) < saveDelay)
            {

                await UniTask.Yield();
                now = DateTime.Now;
            }

            Request.ExecuteObject executeObj = NewExecuteObject("CloudSave.setData");
            executeObj.Parameters = new()
            {
                {"id",slotId },
                {"data",saveDataToSet }
            };
            lastTimeSaved = now;
            executeObj.Encrypt(AesKey, serializerSettings);
            await SendRequest(executeObj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Session of authorised user. Returns null if user is not authorised</returns>
        public static async UniTask<Session> GetSession()
        {

            await sessionTaskSource.Task;

            return session;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotId">0-indexed number of slot</param>
        /// <returns></returns>
        public static async UniTask<string> LoadSlot(int slotId)
        {
            await GetSession();
            if (!IsValidSession)
            {
                Debug.LogError("Cant LoadSlot without session");
                return null;
            }
            slotId += 1;
            Request.ExecuteObject executeObject = NewExecuteObject("CloudSave.loadSlot");
            executeObject.Parameters = new() { { "id", slotId } };
            Response<SaveSlot> resp = await SendRequest<SaveSlot>(executeObject);

            return await LoadSlot(resp.Result["slot"]);
        }
        public static async UniTask<string[]> LoadSlots()
        {
            await GetSession();
            if (!IsValidSession)
            {
                Debug.LogError("Cant LoadSlots without session");
                return null;
            }
            Response<SaveSlot[]> resp = await SendRequest<SaveSlot[]>("CloudSave.loadSlots");

            SaveSlot[] slots = resp.Result["slots"];
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
        private static async UniTask<string> LoadSlot(SaveSlot slot)
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
        private static async UniTask<Session> StartSesion()
        {

            Response<Session> res = await SendRequest<Session>("App.startSession");
            return res.Result["session"];
        }


        private static Request MakeRequest(Request.ExecuteObject executeObject)
        {

            return new() { AppId = AppId, ExecuteObj = executeObject, SessionId = session?.Id };
        }
        private static Request.ExecuteObject NewExecuteObject(string component)
        {
            return new() { Component = component };
        }
        private static UnityWebRequest MakeWebRequest(byte[] bytes)
        {
            UnityWebRequest webRequest = new(GATEWAY_URI, "POST")
            {
                uploadHandler = new UploadHandlerRaw(bytes)

            };
            webRequest.SetRequestHeader("Content-Type", "application/json");
            return webRequest;
        }
        private static UnityWebRequest MakeWebRequest(Request request)
        {

            return MakeWebRequest(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request, serializerSettings)));
        }
        private static UnityWebRequest MakeWebRequest(Request.ExecuteObject executeObject)
        {
            return MakeWebRequest(MakeRequest(executeObject));
        }
        private static async UniTask<Response<ResultDataType>> SendRequest<ResultDataType>(string component)
        {
            return await SendRequest<ResultDataType>(NewExecuteObject(component));
        }

        private static async UniTask SendRequest(Request request)
        {
            UnityWebRequest webRequest = MakeWebRequest(request);


            await webRequest.SendWebRequest().ToUniTask();
        }
        private static async UniTask<Response<ResultDataType>> SendRequest<ResultDataType>(Request.ExecuteObject executeObject)
        {
            Request request = MakeRequest(executeObject);
            return await SendRequest<ResultDataType>(request);
        }
        private static async UniTask SendRequest(Request.ExecuteObject executeObject)
        {
            Request request = MakeRequest(executeObject);
            await SendRequest(request);
        }


        private static async UniTask<Response<ResultDataType>> SendRequest<ResultDataType>(Request request)
        {
            using (UnityWebRequest webRequest = MakeWebRequest(request))
            {

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
                    UnityEngine.Debug.Log(resJson);
                    Response<ResultDataType> response = JsonConvert.DeserializeObject<Response<ResultDataType>>(resJson, serializerSettings);
                    if (response == null)
                    {
                        Debug.LogError("response is null");

                    }
                    if (!response.Success)
                    {
                        if (response.Error != null)
                        {
                            Debug.LogError(response.Error.Message);
                        }
                        else
                        {
                            Debug.LogError("Unknown error");
                        }
                    }

                    return response;
                }

            }
        }
    }
}