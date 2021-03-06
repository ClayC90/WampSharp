using System.IO;
using vtortola.WebSockets;
using WampSharp.Core.Message;
using WampSharp.V2.Binding;

namespace WampSharp.Vtortola
{
    internal class VtortolaWampBinaryConnection<TMessage> : VtortolaWampConnection<TMessage>
    {
        private readonly IWampBinaryBinding<TMessage> mBinding;

        public VtortolaWampBinaryConnection(WebSocket connection, IWampBinaryBinding<TMessage> binding) :
            base(connection)
        {
            mBinding = binding;
        }

        protected override WampMessage<TMessage> ParseMessage(WebSocketMessageReadStream readStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                readStream.CopyTo(memoryStream);
                byte[] bytes = memoryStream.ToArray();
                WampMessage<TMessage> result = mBinding.Parse(bytes);
                return result;
            }
        }

        public override void Send(WampMessage<TMessage> message)
        {
            using (WebSocketMessageWriteStream stream = 
                mWebsocket.CreateMessageWriter(WebSocketMessageType.Binary))
            {
                byte[] raw = mBinding.Format(message);
                stream.Write(raw, 0, raw.Length);
            }
        }
    }
}