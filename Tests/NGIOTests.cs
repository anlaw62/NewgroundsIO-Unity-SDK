using NUnit.Framework;
using UnityEditor;
using Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
namespace Newgrounds.Tests
{
    public class NGIOTests
    {
        private NGIO ngio;
        [SetUp]
        public void SetUp()
        {
            if(NGIO.Instance != null) NGIO.Instance.Dispose();
            JObject appInfo = JObject.Parse(AssetDatabase.LoadAssetAtPath<TextAsset>( AssetDatabase.FindAssets("App").Select(g=>AssetDatabase.GUIDToAssetPath(g)).First()).text);

            ngio = NGIO.Init(appInfo["AppId"].ToObject<string>(), appInfo["AesKey"].ToObject<string>(), appInfo["SessionId"].ToObject<string>());
        }
        [Test]
        public async Task TestGetMedals()
        {

            Medal[] medals = await ngio.GetMedals();
  
            void AssertMedal(int idx, bool secret = false)
            {
                Medal medal = medals[idx];
                void AssertPropValid(bool flag,string name)
                {
                    Assert.True(flag, $"{name} of medal {idx} is incorect");
                }
                AssertPropValid(medal.Id != default, "id");
                AssertPropValid(medal.Description == $"descr{idx+1}", "description");
                AssertPropValid(medal.Name == $"md{idx+1}", "name");
                AssertPropValid(!string.IsNullOrEmpty(medal.IconUrl), "iconUrl");
                AssertPropValid(medal.Secret == secret, "secret");
                AssertPropValid(medal.Difficulty == idx, "difficulty");
                AssertPropValid(medal.Unlocked == false || medal.Value != 0, "value");
            }

            AssertMedal(0);
            AssertMedal(1);
            AssertMedal(2, true);

        }

        [Test]
        public async Task UnlockMedal()
        {

            await UniTask.WhenAll(ngio.UnlockMedal(83697), ngio.UnlockMedal(83698));
            Medal[] medals = await ngio.GetMedals();
            Assert.True(medals.First(m=>m.Id == 83697).Unlocked,"medal 0 is not unlocked");
            Assert.True(medals.First(m => m.Id == 83698).Unlocked, "medal 1 is not unlocked");
            Assert.False(medals.First(m => m.Id == 83699).Unlocked, "medal 2 is  unlocked, this cant be");

        }
        [Test,TestCase("jsjs","38=")]
        public async Task TestSaveLoad(string save1,string save2)
        {
            string[] dataToSave = new string[2] {save1,save2};
            int saveCount = dataToSave.Length;
          for(int i = 0; i < saveCount; i++)
            {
              await  ngio.SaveSlot(i, dataToSave[i]);
            }
            string[] saved = await ngio.LoadSlots();
            for(int i = 0; i < saveCount; i++)
            {
                Assert.True(saved[i] == dataToSave[i], $"save {i} is incorrect");
            }

        }
    }
}