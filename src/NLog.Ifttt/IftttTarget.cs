using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Config;
using NLog.Ifttt;

namespace NLog.Targets
{
    [Target(TargetName)]
    public class IftttTarget : TargetWithLayout
    {
        private const string DefaultContentType = "application/json";
        private const string TargetName = "Ifttt";
        private const string IftttEndpoint = "https://maker.ifttt.com/trigger/{0}/with/key/{1}";

        /// <summary>
        /// Gets or sets the EventName for Ifttt web hook
        /// <example>https://maker.ifttt.com/trigger/{EventName}/with/key/</example>
        /// </summary>
        [RequiredParameter]
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the secret Key for Ifttt web hook
        /// <example>https://maker.ifttt.com/trigger/event/with/key/{Key}</example>
        /// </summary>
        [RequiredParameter]
        public string Key { get; set; }

        /// <summary>
        /// Specifies location of the <see cref="LogEventInfo"/> layout
        /// </summary>
        public LayoutLocation LayoutLocation { get; set; }

        /// <summary>
        /// Gets or sets value in Value1 location
        /// </summary>
        public string Value1 { get; set; }

        /// <summary>
        /// Gets or sets value in Value2 location
        /// </summary>
        public string Value2 { get; set; }

        /// <summary>
        /// Gets or sets value in Value3 location
        /// </summary>
        public string Value3 { get; set; }

        protected override void Write(LogEventInfo logEvent) => WriteInternal(logEvent).Wait();

        protected override void Write(AsyncLogEventInfo asyncLogEvent) =>
            WriteInternal(asyncLogEvent.LogEvent)
                .ContinueWith((task, state) =>
                    asyncLogEvent.Continuation(task.Exception), null);

        private async Task WriteInternal(LogEventInfo logEvent)
        {
            if (string.IsNullOrWhiteSpace(EventName))
            {
                throw new ArgumentException($"{nameof(EventName)} cannot be null or empty string.");
            }

            if (string.IsNullOrWhiteSpace(Key))
            {
                throw new ArgumentException($"{nameof(Key)} cannot be null or empty string.");
            }

            var layout = Layout.Render(logEvent);
            
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var payload = ComposeBodyContent(logEvent, layout);
                    var response = await httpClient.PostAsync(
                            string.Format(IftttEndpoint, EventName, Key),
                            payload)
                        .ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        InternalLogger.Error(
                            $"Request to Ifttt failed with HTTP:{response.StatusCode} {response.ReasonPhrase}.");
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Error(e, "Exception occurent during sending request to the Ifttt web hook.");
                throw;
            }
        }

        private HttpContent ComposeBodyContent(LogEventInfo logEvent, string layout)
        {
            var value1 = RenderLogEvent(Value1, logEvent);
            var value2 = RenderLogEvent(Value2, logEvent);
            var value3 = RenderLogEvent(Value3, logEvent);

            switch (LayoutLocation)
            {
                case LayoutLocation.Value1:
                    value1 = layout;
                    break;
                case LayoutLocation.Value2:
                    value2 = layout;
                    break;
                case LayoutLocation.Value3:
                    value3 = layout;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(LayoutLocation));
            }

            var sb = new StringBuilder();
            {
                sb.Append("{");
                sb.Append($"\"value1\":\"{value1}\",");
                sb.Append($"\"value2\":\"{value2}\",");
                sb.Append($"\"value3\":\"{value3}\"");
                sb.Append("}");
            }

            return new StringContent(sb.ToString(), Encoding.UTF8, DefaultContentType);
        }
    }
}
