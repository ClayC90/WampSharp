﻿using Castle.DynamicProxy;
using WampSharp.Core.Message;

namespace WampSharp.Core.Proxy
{
    /// <summary>
    /// An interceptor that sends raw <see cref="WampMessage{TMessage}"/> to the wire.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class WampRawOutgoingInterceptor<TMessage> : IInterceptor
    {
        private readonly IWampOutgoingMessageHandler<TMessage> mOutgoingHandler;

        /// <summary>
        /// Initializes a new instance of <see cref="WampRawOutgoingInterceptor{TMessage}"/>.
        /// </summary>
        /// <param name="outgoingHandler">The <see cref="IWampOutgoingMessageHandler{TMessage}"/>
        /// used to write messages to the wire.</param>
        public WampRawOutgoingInterceptor(IWampOutgoingMessageHandler<TMessage> outgoingHandler)
        {
            mOutgoingHandler = outgoingHandler;
        }

        /// <summary>
        /// <see cref="IInterceptor.Intercept"/>
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(IInvocation invocation)
        {
            WampMessage<TMessage> message = invocation.Arguments[0] as WampMessage<TMessage>;
            mOutgoingHandler.Handle(message);
        }
    }
}