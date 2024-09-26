using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace EventLogger
{
    public class EventMessageHandler
    {
        private readonly GameEvent[] messageData;
        private UnityWebRequestAsyncOperation requestOperation;
        private string serverUrl;

        public GameEvent[] MessageData => messageData;
        public UnityAction<EventMessageHandler> onComplete;

        public bool isDone => requestOperation == null ? false : requestOperation.isDone;

        public bool isSuccess
        {
            get
            {
                if (!isDone) return false;
                return requestOperation.webRequest.responseCode == 200;
            }
        }

        public EventMessageHandler(string url, GameEvent[] data)
        {
            serverUrl = url;
            messageData = data;
        }

        public void Send()
        {
            var request = new UnityWebRequest(serverUrl, "POST");
            request.SetRequestHeader("Content-Type", "application/json");
            string jsonString = JsonConvert.SerializeObject(new EventMessage(messageData));
            Debug.Log(jsonString);
            byte[] jsonData = new System.Text.UTF8Encoding().GetBytes(jsonString);
            request.uploadHandler = new UploadHandlerRaw(jsonData);
            request.downloadHandler = new DownloadHandlerBuffer();
            requestOperation = request.SendWebRequest();
            requestOperation.completed += OnRequestDone;
        }

        private void OnRequestDone(AsyncOperation operation)
        {
            onComplete?.Invoke(this);
        }
    }
}