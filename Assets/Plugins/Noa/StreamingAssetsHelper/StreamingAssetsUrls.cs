using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

namespace Noa
{
    internal sealed class StreamingAssetsUrl
#if UNITY_EDITOR
        : IPreprocessBuildWithReport
#endif
    {
        private const string INFO_TXT = "StreamingAssetsUrl.txt";
        private const int Offset = 5;
        private static char Separator { get { return '\n'; } }

        /// <summary>
        /// StreamingAssetsに含まれているファイルを読み込む
        /// </summary>
        public static IEnumerator Read(Action<string[]> callback)
        {
            string infoTxtPath = Path.Combine(Application.streamingAssetsPath, INFO_TXT);

            string infoTxt = string.Empty;

            // 読み込む
            var reqest = UnityWebRequest.Get(infoTxtPath);

            yield return reqest.SendWebRequest();

            if (!reqest.isNetworkError && string.IsNullOrEmpty(reqest.error))
            {
                byte[] encInfoData = reqest.downloadHandler.data;

                byte[] decomp = Decompress(encInfoData);

                byte[] decInfoData = OffSetBytes(decomp, Offset);

                infoTxt = Encoding.UTF8.GetString(decInfoData);

                Debug.Log($"Offset: {Offset}, decSize: {decInfoData.Length}, TxtLength: {infoTxt.Length}\n" + infoTxt);
            }

            // 配列に変換を行う
            string[] infos = infoTxt.Split(Separator);

            if (callback != null)
            {
                callback(infos);
            }
        }

        private static byte[] OffSetBytes(byte[] bytes, int offset)
        {
            int dataSize = bytes.Length;

            byte[] decInfoData = new byte[dataSize];

            // 複合化を行う
            if (dataSize > 0)
            {
                for (int i = 0; i < dataSize; i++)
                {
                    int idx = (i + offset + dataSize) % dataSize;
                    decInfoData[idx] = bytes[i];
                }
            }

            return decInfoData;
        }

        /// <summary>
        /// 解凍
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static byte[] Decompress(byte[] bytes)
        {
            return bytes;
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
                .Where(w => (File.GetAttributes(w) & FileAttributes.Hidden) != FileAttributes.Hidden)
                .Select(s => s.Replace(Application.streamingAssetsPath + Path.DirectorySeparatorChar.ToString(), string.Empty))
                .OrderBy(o => o)
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

            // 圧縮を行う
            byte[] compInfoData = Compress(decInfoData);

            // 暗号化を行った場合のTxtファイル
            byte[] encInfoData = OffSetBytes(compInfoData, -1 * Offset);

            // テキストファイルの内容をログへ出力する
            Debug.Log($"FileNum: {infos.Length}, Offset: {Offset}, DataSize: {dataSize} -> compSize: {encInfoData.Length}, TxtLength: {txt.Length}\n" + txt);

            Debug.Log("複合化テスト\n" + Encoding.UTF8.GetString(OffSetBytes(Decompress(encInfoData), Offset)));
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

        /// <summary>
        /// 圧縮
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static byte[] Compress(byte[] bytes)
        {
            return bytes;
        }

        /// <summary>
        /// 一番最後に実行する
        /// </summary>
        public int callbackOrder { get { return int.MaxValue; } }

        /// <summary>
        /// ビルドする前に実行される
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            Write();

            AssetDatabase.Refresh();
        }
#endif
    }
}