using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
namespace Newgrounds.Tests
{
    public class NGIOTests
    {
   
        [SetUp]
        public void SetUp()
        {
          
            JObject appInfo = JObject.Parse(AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.FindAssets("App").Select(g => AssetDatabase.GUIDToAssetPath(g)).First()).text);

            NGIO.Init(appInfo["AppId"].ToObject<string>(), appInfo["AesKey"].ToObject<string>(), appInfo["SessionId"].ToObject<string>());
        }
        [Test]
        public async Task TestGetMedals()
        {

            Medal[] medals = await NGIO.GetMedals();

            void AssertMedal(int idx, bool secret = false)
            {
                Medal medal = medals[idx];
                void AssertPropValid(bool flag, string name)
                {
                    Assert.True(flag, $"{name} of medal {idx} is incorect");
                }
                AssertPropValid(medal.Id != default, "id");
                AssertPropValid(medal.Description == $"descr{idx + 1}", "description");
                AssertPropValid(medal.Name == $"md{idx + 1}", "name");
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

            await UniTask.WhenAll(NGIO.UnlockMedal(83697), NGIO.UnlockMedal(83698));
            Medal[] medals = await NGIO.GetMedals();
            Assert.True(medals.First(m => m.Id == 83697).Unlocked, "medal 0 is not unlocked");
            Assert.True(medals.First(m => m.Id == 83698).Unlocked, "medal 1 is not unlocked");
            Assert.False(medals.First(m => m.Id == 83699).Unlocked, "medal 2 is  unlocked, this cant be");

        }
        [Test, TestCase("BVCs", "{vb==ssjw7tyw7t13sahjdgSAHJfdgdsKFGKDSU33333fsasdasda9dggggg}")]
        public async Task TestSaveLoad(string save1, string save2)
        {
            string[] dataToSave = new string[2] { save1, save2 };
            int saveCount = dataToSave.Length;

            for (int i = 0; i < saveCount; i++)
            {
                NGIO.SaveSlot(i, "nnnnnn").Forget();
                await NGIO.SaveSlot(i, dataToSave[i]);
            }
            string[] saved = await NGIO.LoadSlots();
            for (int i = 0; i < saveCount; i++)
            {
                Assert.True(saved[i] == dataToSave[i], $"save {i} is incorrect,saved:{saved[i]}");
            }

        }
        [Test, TestCase(14734, 847743939), TestCase(14735, 1099)]
        public async Task PostScoreTest(int id, int score)
        {

            await NGIO.PostScore(id, score);
            Score[] scores = await NGIO.GetScores(id, 5);
            Assert.True(scores.FirstOrDefault(s => s.Value == score) != null, "score hasnt been set");
        }
        [Test, TestCase(14734)]
        public async Task GetScoresTest(int id)
        {
            await NGIO.PostScore(id, 1);
            Score[] scores = await NGIO .GetScores(id, 5);
            Assert.True(scores != null, "scores is null");
            Assert.True(scores.Length != 0, "score array is empty");
            foreach (Score score in scores)
            {
                Assert.True(score != null, "score is null");
                Assert.False(string.IsNullOrEmpty(score.FormattedValue), "formatted value is null or empty");
                User user = score.User;
                Assert.True(user != null, "user is null");
                Assert.False(string.IsNullOrEmpty(user.Name), "username is empty or null");
                Assert.True(user.Url != null, "user url is null");
                Assert.True(user.Id != 0, "user id is invalid");

            }
        }

    }
}