using System.Collections.Generic;

namespace I2.Loc
{
    using TranslationDictionary = Dictionary<string, TranslationQuery>;

    public class TranslationJob_Main : TranslationJob
    {
        TranslationJob_Baidu mBaidu;

        TranslationDictionary _requests;
        GoogleTranslation.fnOnTranslationReady _OnTranslationReady;
        public string mErrorMessage;

        public TranslationJob_Main(TranslationDictionary requests, GoogleTranslation.fnOnTranslationReady OnTranslationReady)
        {
            _requests = requests;
            _OnTranslationReady = OnTranslationReady;

            mBaidu = new TranslationJob_Baidu(requests, OnTranslationReady);
        }

        public override eJobState GetState()
        {
            if (mBaidu != null)
            {
                var state = mBaidu.GetState();
                switch (state)
                {
                    case eJobState.Running: return eJobState.Running;
                    case eJobState.Succeeded:
                        {
                            mJobState = eJobState.Succeeded;
                            mBaidu.Dispose();
                            mBaidu = null;
                            break;
                        }
                    case eJobState.Failed:
                        {
                            mErrorMessage = mBaidu.ErrorMessage;
                            mBaidu.Dispose();
                            mBaidu = null;
                            mJobState = eJobState.Failed;
                            break;
                        }
                }
            }

            return mJobState;
        }

        public override void Dispose()
        {
            if (mBaidu != null) mBaidu.Dispose();
            mBaidu = null;
        }
    }
}
