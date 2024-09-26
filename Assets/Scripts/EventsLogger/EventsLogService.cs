using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EventLogger
{
    public class EventsLogService : MonoBehaviour
    {
        [SerializeField] private string serverUrl;
        [SerializeField] private float cooldownBeforeSend;
        [SerializeField] private float saveDataInterval;

        private float cooldownTimer;
        private float saveTimer;
        private readonly List<GameEvent> eventsBuffer = new List<GameEvent>();
        private readonly List<GameEvent> dataInProcess = new List<GameEvent>();
        private readonly HashSet<EventMessageHandler> handlers = new HashSet<EventMessageHandler>();

        private const string prefsKey = "eventsBuffer";

        public void TrackEvent(string type, string data)
        {
            eventsBuffer.Add(new GameEvent(type, data));
        }

        private void Start()
        {
            LoadBuffer();
        }

        private void Update()
        {
            UpdateTimers();
        }

        private void OnApplicationQuit()
        {
            SaveUnsent();
        }

        private void UpdateTimers()
        {
            saveTimer += Time.deltaTime;

            if (eventsBuffer.Any()) cooldownTimer += Time.deltaTime;

            if (cooldownTimer >= cooldownBeforeSend)
            {
                SendMessage();
            }

            if (saveTimer >= saveDataInterval)
            {
                SaveUnsent();
            }
        }

        private void SendMessage()
        {
            if (eventsBuffer.Any())
            {
                var handler = new EventMessageHandler(serverUrl, eventsBuffer.ToArray());
                handlers.Add(handler);
                handler.onComplete += OnMessageComplete;
                dataInProcess.AddRange(eventsBuffer);
                eventsBuffer.Clear();
                handler.Send();
            }
            cooldownTimer = 0;
        }

        private void OnMessageComplete(EventMessageHandler handler)
        {
            if (!handler.isSuccess) //return data to buffer to resend
            {
                eventsBuffer.AddRange(handler.MessageData);
            }

            dataInProcess.RemoveAll((ev) => handler.MessageData.Contains(ev));
            handlers.Remove(handler);
            handler.onComplete -= OnMessageComplete;
            SaveUnsent();
        }

        private void SaveUnsent()
        {
            PlayerPrefs.SetString(prefsKey, JsonConvert.SerializeObject(eventsBuffer.Union(dataInProcess).ToArray()));
            PlayerPrefs.Save();
            saveTimer = 0;
        }

        private void LoadBuffer()
        {
            if (!PlayerPrefs.HasKey(prefsKey)) return;
            var savedData = JsonConvert.DeserializeObject(PlayerPrefs.GetString(prefsKey), typeof(GameEvent[])) as GameEvent[];
            if (savedData == null)
            {
                Debug.LogWarning($"[EventLogger] Can't deserialize data from PlyerPrefs");
                return;
            }

            eventsBuffer.AddRange(savedData);
        }
    }
}