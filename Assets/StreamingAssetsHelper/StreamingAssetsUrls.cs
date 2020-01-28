using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Noa
{
    internal sealed class StreamingAssetsUrl
    {
        private const string INFO_TXT = "StreamingAssetsUrl.txt";
        private const int Offset = 0;
        private const char Separator = '\n';

        /// <summary>
        /// StreamingAssetsに含まれているファイルを読み込む
        /// </summary>
        public static IEnumerator Read(Action<string[]> callback)
        {
            string infoTxtPath = Path.Combine(Application.streamingAssetsPath, INFO_TXT);

            string infoTxtdata = string.Empty;

            // 読み込む
            var reqest = UnityWebRequest.Get(infoTxtPath);

            yield return reqest.SendWebRequest();

            Debug.Log(reqest.error);

            if (string.IsNullOrEmpty(reqest.error))
            {
                byte[] encInfoData = reqest.downloadHandler.data;

                int dataSize = encInfoData.Length;

                byte[] decInfoData = new byte[dataSize];

                // 複合化を行う
                if (dataSize > 0)
                {
                    for (int i = 0; i < dataSize; i++)
                    {
                        int idx = (i - Offset) % dataSize;
                        decInfoData[idx] = encInfoData[i];
                    }
                }

                infoTxtdata = Encoding.UTF8.GetString(decInfoData);
            }

            // 配列に変換を行う
            string[] infos = infoTxtdata.Split(Separator);

            if (callback != null)
            {
                callback(infos);
            }
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Noa/StreamingAssets/InfosFileCreate", false, 1)]
        public static void Write()
        {
            // 全階層のディレクトリからファイルの一覧を取得する
            string[] infos = Directory.GetFiles(Application.streamingAssetsPath, "*", SearchOption.AllDirectories)
                // metaファイルを除く
                .Where(w => Path.GetExtension(w) != ".meta")
                // 自分のTextファイルを除く
                .Where(w => Path.GetFileName(w) != INFO_TXT)
                .ToArray();

            // Txtファイルへ変換を行う
            string txt = string.Empty;
            if (infos != null)
            {
                txt = string.Join(Separator.ToString(), infos);
            }

            // TxtをByte配列へ変換する
            byte[] decInfoData = Encoding.UTF8.GetBytes(txt);

            int dataSize = decInfoData.Length;

            // 暗号化を行った場合のTxtファイル
            byte[] encInfoData = new byte[dataSize];

            // 暗号化を行う
            if (dataSize > 0)
            {
                for (int i = 0; i < dataSize; i++)
                {
                    int idx = (i + Offset) % dataSize;
                    decInfoData[idx] = encInfoData[i];
                }
            }

            // テキストファイルの内容をログへ出力する
            Debug.Log($"FileNum: {infos.Length}, Offset: {Offset}, byteSize: {dataSize} -> encSize: {encInfoData.Length}, TxtLength: {txt.Length}\n" + txt);

            // 保存パス
            string txtPath = Path.Combine(Application.streamingAssetsPath, INFO_TXT);

            // すでに存在している場合は削除を行う
            try
            {
                if (File.Exists(txtPath))
                {
                    File.Delete(txtPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw e;
            }

            // 新しくファイルを生成する
            try
            {
                if (dataSize > 0)
                {
                    File.WriteAllBytes(txtPath, encInfoData);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw e;
            }
        }
#endif
    }
}