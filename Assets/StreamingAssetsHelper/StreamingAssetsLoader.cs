﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Noa
{
    /// <summary>
    /// StreamingAssetsを読み込むためのラッパークラス
    /// </summary>
    public class StreamingAssetsLoader : MonoBehaviour
    {
        private static StreamingAssetsLoader ins = null;

        /// <summary>
        /// Assetの一覧を格納する
        /// </summary>
        public string[] mUrls;

        /// <summary>
        /// 初期化処理
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeLoad()
        {
            GameObject go = new GameObject(typeof(StreamingAssetsLoader).FullName);
            ins = go.AddComponent<StreamingAssetsLoader>();

            if (IsHelper)
            {
                ins.StartCoroutine(StreamingAssetsUrl.Read(infos =>
                {
                    Debug.Log(string.Join("\n", infos));
                    ins.mUrls = infos;
                }));

            }
        }

        /// <summary>
        /// 初期化処理が完了しているかどうか
        /// </summary>
        public static bool IsInitialize
        {
            get
            {
                if (ins != null)
                {
                    return true;
                }

                return false;
            }
        }

        public static string[] Urls
        {
            get
            {
                if (IsHelper)
                {
                    return (ins.mUrls.Clone() as string[]);
                }

                else
                {
                    string[] infos = Directory.GetFiles(Application.streamingAssetsPath, "*", SearchOption.AllDirectories);

                    return infos;
                }
            }
        }

        /// <summary>
        /// Loaderを使うかどうかの判定
        /// </summary>
        /// <value></value>
        private static bool IsHelper
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        return true;

                    default:
                        break;
                }

                return false;
            }
        }

        // StreamingAssetsパスであるか確認を行う
        private static bool IsStreamingAssetsPath(string path)
        {
            if (path != null)
            {
                // StreamingAssetsのパスである場合
                if (path.IndexOf(UnityEngine.Application.streamingAssetsPath) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerator ReadAllBytes(string path, Action<byte[]> callback)
        {
            // ファイルのByteサイズ
            byte[] bytes = null;

            // ファイルがIOで読み込めない場合の処理
            if (IsHelper)
            {
                var reqest = UnityEngine.Networking.UnityWebRequest.Get(path);

                yield return reqest.SendWebRequest();

                if (string.IsNullOrEmpty(reqest.error))
                {
                    bytes = reqest.downloadHandler.data;
                }
            }

            // IOで読み込みが可能な場合
            else
            {
                if (File.Exists(path))
                {
                    bytes = File.ReadAllBytes(path);
                }
            }

            if (callback != null)
            {
                callback(bytes);
            }
        }

        public static bool FileExists(string path)
        {
            if (IsHelper)
            {
                int length = ins.mUrls.Length;
                for (int i = 0; i < length; i++)
                {

                    string url = ins.mUrls[i];
                    if (url == path)
                    {
                        return true;
                    }

                    // StreamingAssetsパスへ変換を行う
                    string saPath = ToStreamingAssetsPath(path);

                    // StreamingAssetsパスで一致するか確認を行う
                    if (ToStreamingAssetsPath(url) == path)
                    {
                        return true;
                    }
                }

                return false;
            }

            return System.IO.Directory.Exists(path);
        }

        private static string ToStreamingAssetsPath(string path)
        {
            // Nullの場合の処理を追加する
            if (path == null)
            {
                path = string.Empty;
            }

            // StreamingAssetsのパスの場合はそのまま返す
            if (IsStreamingAssetsPath(path))
            {
                return path;
            }

            return Path.Combine(UnityEngine.Application.streamingAssetsPath, path);
        }

        /// <summary>
        /// ファイルが存在するか確認をする
        /// </summary>
        public static bool DirecyoryExists(string path)
        {
            if (IsHelper)
            {
                return false;
            }

            return System.IO.Directory.Exists(path);
        }


    }
}