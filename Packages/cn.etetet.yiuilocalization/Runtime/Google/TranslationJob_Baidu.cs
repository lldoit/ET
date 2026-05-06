using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace I2.Loc
{
    using TranslationDictionary = Dictionary<string, TranslationQuery>;

    public sealed class TranslationJob_Baidu : TranslationJob_WWW
    {
        private readonly TranslationDictionary requests;
        private readonly GoogleTranslation.fnOnTranslationReady onTranslationReady;
        private readonly string[] keys;
        private int queryIndex;
        private int targetIndex;
        private string currentKey;
        public string ErrorMessage { get; private set; }

        public TranslationJob_Baidu(TranslationDictionary requests, GoogleTranslation.fnOnTranslationReady onTranslationReady)
        {
            this.requests = requests;
            this.onTranslationReady = onTranslationReady;
            keys = requests.Keys.ToArray();
            StartNext();
        }

        public override eJobState GetState()
        {
            if (www == null || !www.isDone)
            {
                return mJobState;
            }

            var response = Encoding.UTF8.GetString(www.downloadHandler.data);
            var requestError = www.error;
            www.Dispose();
            www = null;

            ProcessResult(response, requestError);
            return mJobState;
        }

        private void StartNext()
        {
            if (queryIndex >= keys.Length)
            {
                Complete(null, eJobState.Succeeded);
                return;
            }

            currentKey = keys[queryIndex];
            var query = requests[currentKey];
            if (targetIndex >= query.TargetLanguagesCode.Length)
            {
                queryIndex++;
                targetIndex = 0;
                StartNext();
                return;
            }

            StartRequest(query);
        }

        private void StartRequest(TranslationQuery query)
        {
            var from = BaiduTranslationUtility.GetBaiduLanguageCode(query.LanguageCode);
            var to = BaiduTranslationUtility.GetBaiduLanguageCode(query.TargetLanguagesCode[targetIndex]);
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || to == "auto")
            {
                Complete($"Baidu translate does not support language: {query.LanguageCode}->{query.TargetLanguagesCode[targetIndex]}", eJobState.Failed);
                return;
            }

            var text = GoogleTranslation.TitleCase(query.Text) == query.Text ? query.Text.ToLowerInvariant() : query.Text;
            if (text.Length > BaiduTranslationSettings.MaxChars)
            {
                Complete(string.Format("Baidu translate text length {0} exceeds max chars {1}", text.Length, BaiduTranslationSettings.MaxChars), eJobState.Failed);
                return;
            }

            var salt = DateTime.UtcNow.Ticks.ToString();
            var sign = BaiduTranslationUtility.GenerateSign(BaiduTranslationSettings.AppId, text, salt, BaiduTranslationSettings.SecretKey);
            www = UnityWebRequest.Get(BaiduTranslationUtility.BuildRequestUrl(text, from, to, salt, sign, BaiduTranslationSettings.AppId));
            I2Utils.SendWebRequest(www);
        }

        private void ProcessResult(string response, string requestError)
        {
            if (!string.IsNullOrEmpty(requestError))
            {
                Complete(requestError, eJobState.Failed);
                return;
            }

            if (!BaiduTranslationUtility.TryParseResponse(response, out var translation, out var parseError))
            {
                Complete(parseError, eJobState.Failed);
                return;
            }

            SaveResult(BaiduTranslationUtility.RestoreTags(translation, requests[currentKey].Tags));
            targetIndex++;
            StartNext();
        }

        private void SaveResult(string translation)
        {
            var query = requests[currentKey];
            if (query.Results == null)
            {
                query.Results = new string[query.TargetLanguagesCode.Length];
            }

            query.Results[targetIndex] = translation;
            requests[currentKey] = query;
        }

        private void Complete(string error, eJobState state)
        {
            ErrorMessage = error;
            if (onTranslationReady != null)
            {
                onTranslationReady(requests, error);
            }

            mJobState = state;
        }
    }
}
