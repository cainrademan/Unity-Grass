#if UNITY_EDITOR

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Quixel {
    public class QXLServer {
        private TcpListener tcpListener;
        private Thread tcpListenerThread;
        private TcpClient connectedTcpClient;

        private bool isRunning;

        public List<string> jsonData = new List<string> ();

        // Use this for initialization
        public void StartServer () {
            if(!isRunning)
            {
                // Start TcpServer background thread 		
                tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
                tcpListenerThread.IsBackground = true;
                tcpListenerThread.Start();
                isRunning = true;
            }
        }

        public void EndServer () {
            isRunning = false;
            tcpListener.Stop ();
            tcpListenerThread.Abort();
            Debug.Log ("Quixel Bridge Plugin - Status: Disabled.");
        }

        private void ListenForIncommingRequests () {
            try {
                tcpListener = new TcpListener (IPAddress.Parse ("127.0.0.1"), 13081);
                tcpListener.Start ();
                Debug.Log ("Quixel Bridge Plugin - Status: Enabled.");
                Byte[] bytes = new Byte[4096];
                while (true) {
                    using (connectedTcpClient = tcpListener.AcceptTcpClient ()) {
                        using (NetworkStream stream = connectedTcpClient.GetStream ()) {
                            int length;
                            string clientMessage = "";
                            while ((length = stream.Read (bytes, 0, bytes.Length)) != 0) {
                                try {
                                    byte[] incommingData = new byte[length];
                                    Array.Copy(bytes, 0, incommingData, 0, length);
                                    UTF8Encoding encodingUnicode = new UTF8Encoding();
                                    clientMessage += encodingUnicode.GetString(incommingData);
                                } catch (Exception ex)
                                {
                                    Debug.Log("Bridge Plugin Exception::Error::Encoding json data.");
                                    Debug.Log("Exception: " + ex.ToString());
                                }
                            }
                            jsonData.Add (clientMessage);
                        }
                    }
                }
            } catch (SocketException socketException) {
                Debug.Log ("Bridge Plugin - Status: Stopped.");
                Debug.Log ("SocketException " + socketException.ToString ());
            }
        }
    }

    [InitializeOnLoad]
    [ExecuteInEditMode]
    public class MegascansBridgeLink {
        static private bool isRunning = false;
        static private QXLServer listener;
        static private MegascansImporter mi;
        static MegascansBridgeLink () {
            listener = new QXLServer ();
            EditorApplication.update += ImportTheThing;
            mi = ScriptableObject.CreateInstance<MegascansImporter> ();
        }

        public static void ToggleServer(bool runServer = true)
        {
            if(runServer) //Start server
            {
                if (isRunning)
                {
                    try
                    {
                        if (isRunning)
                        {
                            listener.EndServer();
                            isRunning = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.ToString());
                    }
                }
                isRunning = true;
                listener.StartServer();
            }
            else //Stop server
            {
                try
                {
                    if (isRunning)
                    {
                        listener.EndServer();
                        isRunning = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
            }
        }

        static void ImportTheThing () {
            if (listener != null && isRunning) {
                if (listener.jsonData.Count > 0) {
                    try {
                        string jArray = listener.jsonData[0];
                        Newtonsoft.Json.Linq.JArray testArray = Newtonsoft.Json.Linq.JArray.Parse(jArray);
                        List<Newtonsoft.Json.Linq.JObject> objectList = new List<Newtonsoft.Json.Linq.JObject>();
                        for (int i = 0; i < testArray.Count; ++i)
                        {
                            JObject assetObj = testArray[i].ToObject<Newtonsoft.Json.Linq.JObject>();
                            objectList.Add(assetObj);
                        }
                        string lastFolderPath = null;
                        for (int i = 0; i < objectList.Count; ++i)
                        {
                            Debug.Log(objectList[i]);
                            lastFolderPath = mi.ImportMegascansAssets(objectList[i]);
                        }
                        //Highlight the last imported asset at the end of the import operation.
                        if (lastFolderPath != null)
                        {
                            UnityEngine.Object folder = AssetDatabase.LoadAssetAtPath(lastFolderPath, typeof(UnityEngine.Object));
                            Selection.activeObject = folder;
                            EditorGUIUtility.PingObject(folder);
                        }

                        listener.jsonData.RemoveAt(0);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Bridge Plugin Exception::Error::Parsing json data.");
                        Debug.Log("Bridge Plugin::Data::Received JSON Data: " + listener.jsonData);
                        Debug.Log("Exception: " + ex.ToString());
                        listener.jsonData.RemoveAt(0);
                    }
                }
            }
        }
    }
}

#endif