﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using IWebsocketClientLite.PCL;
using WebsocketClientLite.PCL.Model;
using System.Threading;
using System.Reactive.Disposables;
using WebsocketClientLite.PCL.Helper;
using static WebsocketClientLite.PCL.Helper.DataframeParsing;


namespace WebsocketClientLite.PCL.Service
{
    internal class WebsocketParserHandler : IDisposable
    {
        private readonly TcpConnectionService _tcpConnectionService;

        internal IEnumerable<string> SubprotocolAcceptedNames { get; private set; }

        internal WebsocketParserHandler(
            TcpConnectionService tcpConnectionService)
        {
            _tcpConnectionService = tcpConnectionService;
        }

        internal IObservable<Dataframe> DataframeObservable() => 
            Observable.Create<Dataframe>(async obs =>
            {
                var cts = new CancellationTokenSource();

                var dataframe = await CreateDataframe(_tcpConnectionService, cts.Token)
                    .PayloadBitLenght()
                    .PayloadLenght()
                    .GetPayload();

                if (dataframe is not null)
                {
                    while (!dataframe.FIN)
                    {
                        // Merge fragments into one dataframe.
                        dataframe = await GetNextDataframe(dataframe);
                    }

                    obs.OnNext(dataframe);
                }
                             
                obs.OnCompleted();

                return Disposable.Create(() => cts.Cancel());

                async Task<Dataframe> GetNextDataframe(Dataframe dataframe)
                {
                    var nextDataframe = await GetDataframe();

                    if (nextDataframe is not null)
                    {
                        await nextDataframe.DataStream.CopyToAsync(dataframe.DataStream,
#if !NETSTANDARD2_1
                            81920,
#endif
                            cts.Token);


                        dataframe = dataframe with
                        {
                            FIN = nextDataframe.FIN,
                        };
                    }

                    return dataframe;
                }

                async Task<Dataframe> GetDataframe()
                {
                    var newDataframe = await CreateDataframe(_tcpConnectionService, cts.Token)
                        .PayloadBitLenght()
                        .PayloadLenght()
                        .GetPayload();

                    if (newDataframe is null)
                    {
                        return null;
                    }

                    if (newDataframe.Opcode
                        is OpcodeKind.Text
                        or OpcodeKind.Binary
                        or OpcodeKind.Continuation
                        || newDataframe.Fragment is FragmentKind.Last)
                    {
                        return newDataframe;
                    }
                    else
                    {
                        obs.OnNext(newDataframe);
                    }

                    return null;
                }
            });        
        
        public void Dispose()
        {

        }
    }
}
